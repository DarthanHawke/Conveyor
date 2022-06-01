using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Reflection;
using System.Threading;
using System.Windows.Forms;

namespace Conveyor.View
{
    public partial class ConveyorsForm : Form
    {
        private Controller.ConveyorsController[] F_ccConveyorsMashine = new Controller.ConveyorsController[Program.F_iThreadNumber];
        private Controller.LoaderController F_lclLoadersMashine;

        private Control[] F_controlThread = new Control[Program.F_iThreadNumber];
        private Graphics[] graphicsThread = new Graphics[Program.F_iThreadNumber];
        private Graphics[] graphicsPartsThread = new Graphics[Program.F_iThreadNumber];


        private Models.ChiefEngineer chiefmech = new Models.ChiefEngineer();
        private Models.JuniorMechanic junmech = new Models.JuniorMechanic();
        private Models.SeniorMechanic senmech = new Models.SeniorMechanic();

        Button[] buttonModel = new Button[Program.F_iThreadNumber];

        public ConveyorsForm()
        {
            InitializeComponent();
        }

        Thread[] thread = new Thread[Program.F_iThreadNumber];

        private void initThread(int numThread)
        {
            thread[numThread] = new Thread((obj) => startConveyors(obj));

            thread[numThread].Start(numThread);
        }

        object locker = new object();
        private void startConveyors(object obj)
        {
            int numThread = (int)obj;
            F_ccConveyorsMashine[numThread] = new Controller.ConveyorsController(150 * numThread + 30);
            F_lclLoadersMashine = new Controller.LoaderController();
            F_controlThread[numThread] = new Control();
            graphicsThread[numThread] = CreateGraphics();
            graphicsPartsThread[numThread] = CreateGraphics();
            addConveyor(numThread);



            TimerCallback tmLoad = new TimerCallback(loadTick);
            System.Threading.Timer timerLoad = new System.Threading.Timer(tmLoad, numThread, 0, 1);

            TimerCallback tmConveyors = new TimerCallback(conveyorsTick);
            System.Threading.Timer timerConveyors = new System.Threading.Timer(tmConveyors, numThread, 0, 100);

            while (thread[numThread].IsAlive)
            {

            }

                //addConveyor(numThread);
                /*int delay = 1000; // 1 second
                while (thread[numThread].IsAlive)
                {
                    F_lclLoadersMashine.controlLoad(F_ccConveyorsMashine[numThread].CC_cConveyor);
                    F_ccConveyorsMashine[numThread].conveyorOperation();
                    F_ccConveyorsMashine[numThread].conveyorIsBroken();
                    addLoader(numThread);
                    addParts(numThread);
                    //addConveyor(numThread);
                    Thread.Sleep(delay);
                }*/
            }

        public void conveyorsTick(object obj)
        {
            int numThread = (int)obj;

            lock (locker)
            {
                F_ccConveyorsMashine[numThread].conveyorOperation();
                //F_ccConveyorsMashine[numThread].conveyorIsBroken();
                //graphicsPartsThread[numThread].Clear(Color.White);

                addParts(numThread);
            }

        }


        public void loadTick(object obj)
        {
            int numThread = (int)obj;
            F_lclLoadersMashine.controlLoad(F_ccConveyorsMashine[numThread].CC_cConveyor);
            //addLoader(numThread);
        }


        Bitmap clearPartsImage = new Bitmap(@"../../Resources/clearpart.png");
        Bitmap partsImage = new Bitmap(@"../../Resources/part.png");
        private void addParts(int numThread)
        {
            if (F_ccConveyorsMashine[numThread].CC_cConveyor.C_qConveyor.Count != 0)
            {
                //Models.Parts tryQueue;

                /*foreach (PictureBox obj in this.Controls.OfType<PictureBox>())
                {
                    F_ccConveyorsMashine[numThread].CC_cConveyor.C_qConveyor.TryPeek(out tryQueue);

                    if (obj.Location.X >= Models.Conveyors.C_iStep * 5 + 500
                        && obj.Name == tryQueue.Name)
                    {    
                        Action action = () => Controls.Remove(obj);
                        if (InvokeRequired)
                            Invoke(action);
                        else
                            action();
                    }
                }*/
                graphicsPartsThread[numThread].DrawImage(clearPartsImage, 250, 150 * numThread + 30);
                foreach (var part in F_ccConveyorsMashine[numThread].CC_cConveyor.C_qConveyor)
                {
                    graphicsPartsThread[numThread].DrawImage(partsImage, part.P_iPosX, part.P_iPosY);
                    //tryQueue = new PictureBox();
                    /*tryQueue.Name = "part";
                    tryQueue.Image = Image.FromFile(@"../../Resources/part.png");
                    //tryQueue.Location = part.Location;
                    //tryQueue.Size = part.Size;
                    tryQueue.SizeMode = PictureBoxSizeMode.Zoom;*/
                    /* Action action = () => Controls.Add(tryQueue);
                     if (InvokeRequired)
                         Invoke(action);
                     else
                         action();*/
                }
            }
        }

        Bitmap conveyorImage = new Bitmap(@"../../Resources/conveyor.png");
        private void addConveyor(int numThread)
        {
            graphicsThread[numThread].DrawImage(conveyorImage, 250, 150 * numThread + 30);


            /*PictureBox tryQueue;
            tryQueue = new PictureBox();
            tryQueue.Name = "conveer";
            tryQueue.Image = Image.FromFile(@"../../Resources/conveyor.png");
            tryQueue.Size = new System.Drawing.Size(700, 150);
            tryQueue.Location = new Point(250, 150 * numThread + 30);
            tryQueue.SizeMode = PictureBoxSizeMode.Zoom;

            Action action = () => Controls.Add(tryQueue);
            if (InvokeRequired)
                Invoke(action);
            else
                action();*/
        }


        private void addLoader(int numThread)
        {
            PictureBox tryQueue;
            tryQueue = new PictureBox();
            tryQueue.Name = "loader";
            tryQueue.Image = Image.FromFile(@"../../Resources/loader.png");
            tryQueue.Size = new System.Drawing.Size(120, 180);
            tryQueue.Location = new Point(50, 230);
            tryQueue.SizeMode = PictureBoxSizeMode.Zoom;

            Action action = () => Controls.Add(tryQueue);
            if (InvokeRequired)
                Invoke(action);
            else
                action();
        }


        private void ConveyorsForm_Load(object sender, EventArgs e)
        {
            this.Size = new System.Drawing.Size(1280, 720);
            //initMechanic();
            //initTimer();
            initButton();
        }


        private void initTimer()
        {
            System.Windows.Forms.Timer timerMechanic = new System.Windows.Forms.Timer();
            timerMechanic.Interval = 1000; // 1000 миллисекунд
            timerMechanic.Enabled = true;
            timerMechanic.Tick += timerMechanic_Tick;
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
            buttonModel[0].Click += buttonModelOne_Click;
            buttonModel[1].Click += buttonModelTwo_Click;
            buttonModel[2].Click += buttonModelThree_Click;
            buttonModel[3].Click += buttonModelFour_Click;
        }


        void buttonModelOne_Click(object sender, EventArgs e)
        {
            buttonModel[0].Visible = false;
            initThread(0);
        }


        void buttonModelTwo_Click(object sender, EventArgs e)
        {
            buttonModel[1].Visible = false;
            initThread(1);
        }


        void buttonModelThree_Click(object sender, EventArgs e)
        {
            buttonModel[2].Visible = false;
            initThread(2);
        }


        void buttonModelFour_Click(object sender, EventArgs e)
        {
            buttonModel[3].Visible = false;
            initThread(3);
        }
    }
}
