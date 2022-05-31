using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Threading;
using System.Windows.Forms;

namespace Conveyor.View
{
    public partial class ConveyorsForm : Form
    {
        private const int F_iThreadNumber = 4;
        public delegate void ParameterizedThreadStart(object obj);
        System.Threading.Timer[] timerConveyors = new System.Threading.Timer[F_iThreadNumber];
        object locker = new object();
        private Controller.ConveyorsController[] F_ccConveyorsMashine = new Controller.ConveyorsController[F_iThreadNumber];
        private Controller.LoaderController F_lclLoadersMashine = new Controller.LoaderController();
        private Models.ChiefEngineer chiefmech = new Models.ChiefEngineer();
        private Models.JuniorMechanic junmech = new Models.JuniorMechanic();
        private Models.SeniorMechanic senmech = new Models.SeniorMechanic();
        Button[] buttonModel = new Button[F_iThreadNumber];

        public ConveyorsForm()
        {
            InitializeComponent();
        }

        private void initTread()
        {
            TimerCallback tmConveyors = new TimerCallback(startConveyors);
            for (int i = 0; i < timerConveyors.Length; ++i)
            {
                timerConveyors[i] = new System.Threading.Timer(tmConveyors, i, 0, 1);
            }
        }


        private void startConveyors(object obj)
        {
            lock (locker)
            {
                int i = (int)obj;
                if (F_ccConveyorsMashine[i] != null)
                {
                    if (F_ccConveyorsMashine[i].CC_cConveyor.C_bWorkStatus)
                    {
                        F_ccConveyorsMashine[i].conveyorOperation();
                        if (buttonModel[i] != null)
                        {
                            this.buttonModel[i].Invoke((MethodInvoker)delegate
                            {
                                initParts(i);
                                initConveyor(i);
                            });
                        }
                        F_ccConveyorsMashine[i].conveyorIsBroken();
                    }
                }
            }
        }


        private void initParts(int i)
        {
            if (F_ccConveyorsMashine[i] != null)
            {
                if (F_ccConveyorsMashine[i].CC_cConveyor.C_qConveyor.Count != 0)
                {
                    foreach (PictureBox obj in this.Controls.OfType<PictureBox>())
                    {
                        if (obj.Location.X >= Models.Conveyors.C_iStep * 5 +500
                            && obj.Name == F_ccConveyorsMashine[i].CC_cConveyor.C_qConveyor.Peek().Name)
                        {
                            this.Controls.Remove(obj);
                        }
                    }
                    foreach (var part in F_ccConveyorsMashine[i].CC_cConveyor.C_qConveyor)
                    {
                        this.Controls.Add(part);
                    }
                }
            }
        }


        private void initTimer()
        {
            System.Windows.Forms.Timer timerLoaders = new System.Windows.Forms.Timer();
            System.Windows.Forms.Timer timerMechanic = new System.Windows.Forms.Timer();
            timerLoaders.Interval = 500; // 500 миллисекунд
            timerLoaders.Enabled = true;
            timerLoaders.Tick += timerLoaders_Tick;
            timerMechanic.Interval = 1000; // 1000 миллисекунд
            timerMechanic.Enabled = true;
            timerMechanic.Tick += timerMechanic_Tick;
        }


        private void initConveyor(int i)
        {
            if (F_ccConveyorsMashine[i] != null)
            {
                this.Controls.Add(F_ccConveyorsMashine[i].CC_cConveyor.C_pbConveer);
            }
        }


        private void initLoader()
        {
            F_lclLoadersMashine.initLoader();
            this.Controls.Add(F_lclLoadersMashine.LC_lLoader.L_pbLoader);
        }


        private void initMechanic()
        {
            junmech.initializeMechanic();
            senmech.initializeMechanic();
            chiefmech.initializeMechanic();
            this.Controls.Add(junmech.JM_pbJunMech);
            this.Controls.Add(senmech.SM_pbSenMech);
            this.Controls.Add(chiefmech.CE_pbChiefMech);
        }


        private void initButton()
        {
            for (int i = 0; i < buttonModel.Length; ++i)
            {
                buttonModel[i] = new Button();
                buttonModel[i].Enabled = true;
                buttonModel[i].Visible = true;
                buttonModel[i].Size = new System.Drawing.Size(700, 150);
                buttonModel[i].Location = new Point(250, 150 * i + 30);
                buttonModel[i].Text = "Press button to create new model of conveyor";
                buttonModel[i].Font = new Font("Arial", 24, FontStyle.Bold);
                this.Controls.Add(buttonModel[i]);
            }
            buttonModel[0].Visible = false;
            buttonModel[0].Click += buttonModelOne_Click;
            buttonModel[1].Click += buttonModelTwo_Click;
            buttonModel[2].Click += buttonModelThree_Click;
            buttonModel[3].Click += buttonModelFour_Click;
        }


        void timerLoaders_Tick(object sender, EventArgs e)
        {
            foreach (var model in F_ccConveyorsMashine)
            {
                if (model != null)
                {
                    F_lclLoadersMashine.controlLoad(ref model.CC_cConveyor);
                }
            }
        }


        void timerMechanic_Tick(object sender, EventArgs e)
        {
            foreach (var model in F_ccConveyorsMashine)
            {
                if (model != null)
                {
                    if (!model.CC_cConveyor.C_bWorkStatus)
                    {
                        if (junmech.CE_bBusyness)
                        {
                            junmech.controlRepair(ref model.CC_cConveyor);
                        }
                        else if (senmech.CE_bBusyness)
                        {
                            senmech.controlRepair(ref model.CC_cConveyor);
                        }
                        else if (chiefmech.CE_bBusyness)
                        {
                            chiefmech.controlRepair(ref model.CC_cConveyor);
                        }
                    }
                }
            }
        }


        void buttonModelOne_Click(object sender, EventArgs e)
        {

        }


        void buttonModelTwo_Click(object sender, EventArgs e)
        {
            F_ccConveyorsMashine[1] = new Controller.ConveyorsController(150 * 1 + 30);
            buttonModel[1].Visible = false;
            initConveyor(1);
        }


        void buttonModelThree_Click(object sender, EventArgs e)
        {
            F_ccConveyorsMashine[2] = new Controller.ConveyorsController(150 * 2 + 30);
            buttonModel[2].Visible = false;
            initConveyor(2);
        }


        void buttonModelFour_Click(object sender, EventArgs e)
        {
            F_ccConveyorsMashine[3] = new Controller.ConveyorsController(150 * 3 + 30);
            buttonModel[3].Visible = false;
            initConveyor(3);
        }


        private void ConveyorsForm_Load(object sender, EventArgs e)
        {
            this.Size = new System.Drawing.Size(1280, 720);
            F_ccConveyorsMashine[0] = new Controller.ConveyorsController(30);
            F_lclLoadersMashine.initializeLoaderController();
            initMechanic();
            initConveyor(0);
            initLoader();
            initTimer();
            initButton();
            initTread();
        }
    }
}
