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
        private Controller.ConveyorsController[] F_ccConveyorsMashine;
        private Controller.LoaderController[] F_lclLoadersMashine;
        private Controller.ChiefEngineer F_ceChiefmech;
        private Controller.SeniorMechanic F_smSenmech;
        private Controller.JuniorMechanic F_jmJunmech;

        Thread[] F_thread;
        bool[] F_threadStatus;
        object F_oLockerConveyors;
        object F_oLockerLoaders;
        object F_oLockerMechanic;
        private int F_iBusyThread { get; set; }

        private Graphics[] F_graphicsConveyors;
        private Graphics[] F_graphicsParts;
        private Graphics[] F_graphicsLoaders;
        private Graphics F_graphicsMechanic;

        Button[] F_buttonModelCreate;
        Button[] F_buttonModelDestroy;
        Button[] F_buttonMechanicCreate;
        Button[] F_buttonMechanicDestroy;

        private Bitmap conveyorImage;
        private Bitmap loaderImage;
        private Bitmap clearLoaderImage;
        private Bitmap partsImage;
        private Bitmap clearPartsImage;
        private Bitmap chiefmechImage;
        private Bitmap senmechImage;
        private Bitmap junmechImage;
        private Bitmap clearMechImage;


        private void initializeModels()
        {
            F_ccConveyorsMashine = new Controller.ConveyorsController[Program.F_iThreadNumber];
            F_lclLoadersMashine = new Controller.LoaderController[Program.F_iThreadNumber];

            F_thread = new Thread[Program.F_iThreadNumber];
            F_threadStatus = new bool[Program.F_iThreadNumber];
            F_oLockerConveyors = new object();
            F_oLockerLoaders = new object();
            F_oLockerMechanic = new object();
            F_iBusyThread = -1;

            F_graphicsConveyors = new Graphics[Program.F_iThreadNumber];
            F_graphicsParts = new Graphics[Program.F_iThreadNumber];
            F_graphicsLoaders = new Graphics[Program.F_iThreadNumber];
            F_graphicsMechanic = CreateGraphics();

            F_buttonModelCreate = new Button[Program.F_iThreadNumber];
            F_buttonModelDestroy = new Button[Program.F_iThreadNumber];
            F_buttonMechanicCreate = new Button[3];
            F_buttonMechanicDestroy = new Button[3];
        }


        private void initializeImage()
        {
            conveyorImage = new Bitmap(@"../../Resources/conveyor.png");
            loaderImage = new Bitmap(@"../../Resources/loader.png");
            clearLoaderImage = new Bitmap(@"../../Resources/clearloader.png");
            partsImage = new Bitmap(@"../../Resources/part.png");
            clearPartsImage = new Bitmap(@"../../Resources/clearpart.png");
            chiefmechImage = new Bitmap(@"../../Resources/chiefengineer.png");
            senmechImage = new Bitmap(@"../../Resources/seniormechanic.png");
            junmechImage = new Bitmap(@"../../Resources/juniormechanic.png");
            clearMechImage = new Bitmap(@"../../Resources/clearmechanic.png");
        }


        public ConveyorsForm()
        {
            InitializeComponent();
        }


        private void ConveyorsForm_Load(object sender, EventArgs e)
        {
            this.Size = new System.Drawing.Size(1280, 720);
            initializeModels();
            initializeImage();
            initializeButton();
        }


        private void initializeThread(int numThread)
        {
            F_thread[numThread] = new Thread((obj) => startConveyors(obj));

            F_thread[numThread].Start(numThread);
        }


        private void destructConveyor(int numThread)
        {
            F_threadStatus[numThread] = false;

            if (F_thread[numThread].Join(2000) == true)
            {
                F_graphicsParts[numThread] = null;
                F_graphicsLoaders[numThread] = null;
                F_graphicsConveyors[numThread] = null;
                F_lclLoadersMashine[numThread] = null;
                F_ccConveyorsMashine[numThread] = null;
            }
        }


        private void startConveyors(object obj)
        {
            int numThread = (int)obj;
            F_threadStatus[numThread] = true;
            F_ccConveyorsMashine[numThread] = new Controller.ConveyorsController(160 * numThread + 30);
            F_lclLoadersMashine[numThread] = new Controller.LoaderController(160 * numThread + 30);
            F_graphicsConveyors[numThread] = CreateGraphics();
            F_graphicsLoaders[numThread] = CreateGraphics();
            F_graphicsParts[numThread] = CreateGraphics();
            paintConveyor(numThread);

            TimerCallback tmLoad = new TimerCallback(loadTick);
            System.Threading.Timer timerLoad = new System.Threading.Timer(tmLoad, numThread, 0, 700);

            TimerCallback tmMechanic = new TimerCallback(mechanicTick);
            System.Threading.Timer timerMechanic = new System.Threading.Timer(tmMechanic, null, 0, 1000);

            TimerCallback tmConveyors = new TimerCallback(conveyorsTick);
            System.Threading.Timer timerConveyors = new System.Threading.Timer(tmConveyors, numThread, 0, 100);

            while (F_thread[numThread].IsAlive && F_threadStatus[numThread] == true) { }
            AutoResetEvent waitHandlerLoad = new AutoResetEvent(false);
            AutoResetEvent waitHandlerConveyors = new AutoResetEvent(false);
            timerLoad.Dispose(waitHandlerLoad);
            timerConveyors.Dispose(waitHandlerConveyors);
            waitHandlerLoad.WaitOne();
            waitHandlerConveyors.WaitOne();
        }


        private void loadTick(object obj)
        {
            int numThread = (int)obj;

            if (numThread == F_iBusyThread || F_iBusyThread == -1)
            {
                F_iBusyThread = numThread;

                F_lclLoadersMashine[numThread].controlLoad(F_ccConveyorsMashine[numThread].CC_cConveyor);
                paintLoader(numThread);
                bool checkBusy = true;
                foreach (var LoadMashine in F_lclLoadersMashine)
                {
                    if (LoadMashine != null)
                    {
                        checkBusy = checkBusy && !LoadMashine.LC_lLoader.L_bLoading;
                    }
                }
                if (checkBusy == true)
                {
                    F_iBusyThread = -1;
                }
            }
        }


        private void conveyorsTick(object obj)
        {
            int numThread = (int)obj;

            lock (F_oLockerConveyors)
            {
                if (F_ccConveyorsMashine[numThread].CC_cConveyor.C_bWorkStatus)
                {
                    F_ccConveyorsMashine[numThread].conveyorOperation();
                    F_ccConveyorsMashine[numThread].conveyorIsBroken();
                    paintParts(numThread);
                }
            }
        }


        private void paintParts(int numThread)
        {
            F_graphicsParts[numThread].DrawImage(clearPartsImage, 250, 160 * numThread + 30);
            if (F_ccConveyorsMashine[numThread].CC_cConveyor.C_qConveyor.Count != 0)
            {
                foreach (var part in F_ccConveyorsMashine[numThread].CC_cConveyor.C_qConveyor)
                {
                    F_graphicsParts[numThread].DrawImage(partsImage, part.P_iPosX, part.P_iPosY);
                }
            }
        }


        private void paintConveyor(int numThread)
        {
            F_graphicsConveyors[numThread].DrawImage(conveyorImage, 
                F_ccConveyorsMashine[numThread].CC_cConveyor.C_pbConveer.P_iPosX,
                F_ccConveyorsMashine[numThread].CC_cConveyor.C_pbConveer.P_iPosY);
        }


        private void paintLoader(int numThread)
        {
            lock (F_oLockerLoaders)
            {

                F_graphicsLoaders[numThread].DrawImage(clearLoaderImage, 0, 0);
                F_graphicsLoaders[numThread].DrawImage(loaderImage,
                    F_lclLoadersMashine[numThread].LC_lLoader.L_pbLoader.P_iPosX,
                    F_lclLoadersMashine[numThread].LC_lLoader.L_pbLoader.P_iPosY);
            }
        }


        private void initializeMechanic(int number)
        {    
            switch (number)
            {
                case 0:
                    F_ceChiefmech = new Controller.ChiefEngineer();
                    break;
                case 1:
                    F_smSenmech = new Controller.SeniorMechanic();
                    break;
                case 2:
                    F_jmJunmech = new Controller.JuniorMechanic();
                    break;
            }  
        }


        private void destructMechanic(int number)
        {
            switch (number)
            {
                case 0:
                    F_ceChiefmech = null;
                    break;
                case 1:
                    F_smSenmech = null;
                    break;
                case 2:
                    F_jmJunmech = null;
                    break;
            }
        }


        private void mechanicTick(object sender)
        {
            bool findMechanic = false;
            foreach (var model in F_ccConveyorsMashine)
            {
                if (model != null)
                {
                    if (!model.CC_cConveyor.C_bWorkStatus)
                    {
                        if (F_ceChiefmech != null && findMechanic == false)
                        {
                            if (F_ceChiefmech.CE_bBusyness)
                            {
                                findMechanic = true;
                                F_ceChiefmech.controlRepair(ref model.CC_cConveyor);
                            }
                        }
                        if (F_smSenmech != null && findMechanic == false)
                        {
                            if (F_smSenmech.CE_bBusyness)
                            {
                                findMechanic = true;
                                F_smSenmech.controlRepair(ref model.CC_cConveyor);
                            }
                        }
                        if (F_jmJunmech != null && findMechanic == false)
                        {
                            if (F_jmJunmech.CE_bBusyness)
                            {
                                findMechanic = true;
                                F_jmJunmech.controlRepair(ref model.CC_cConveyor);
                            }
                        }
                    }
                }
            }
            PaintMechanic();
        }


        private void PaintMechanic()
        {
            lock (F_oLockerMechanic)
            {
                if (F_ceChiefmech != null)
                {
                    F_graphicsMechanic.DrawImage(chiefmechImage, F_ceChiefmech.CE_pbChiefMech.P_iPosX, F_ceChiefmech.CE_pbChiefMech.P_iPosY);
                }
                else
                {
                    F_graphicsMechanic.DrawImage(clearMechImage, 1100, 50);
                }
                if (F_smSenmech != null)
                {
                    F_graphicsMechanic.DrawImage(senmechImage, F_smSenmech.SM_pbSenMech.P_iPosX, F_smSenmech.SM_pbSenMech.P_iPosY);
                }
                else
                {
                    F_graphicsMechanic.DrawImage(clearMechImage, 1050, 250);
                }
                if (F_jmJunmech != null)
                {
                    F_graphicsMechanic.DrawImage(junmechImage, F_jmJunmech.JM_pbJunMech.P_iPosX, F_jmJunmech.JM_pbJunMech.P_iPosY);
                }
                else
                {
                    F_graphicsMechanic.DrawImage(clearMechImage, 1100, 500);
                }
            }
        }


        private void initializeButton()
        {
            initializeButtonCreateModel();
            initializeButtonDestroyModel();
            initializeButtonCreateMechanic();
            initializeButtonDestroyMechanic();
        }


        private void initializeButtonCreateModel()
        {
            for (int i = 0; i < F_buttonModelCreate.Length; ++i)
            {
                F_buttonModelCreate[i] = new Button();
                F_buttonModelCreate[i].Enabled = true;
                F_buttonModelCreate[i].Visible = true;
                F_buttonModelCreate[i].Size = new System.Drawing.Size(700, 150);
                F_buttonModelCreate[i].Location = new Point(250, 160 * i + 30);
                F_buttonModelCreate[i].Text = "Press button to create new model of conveyor";
                F_buttonModelCreate[i].Font = new Font("Arial", 24, FontStyle.Bold);
                this.Controls.Add(F_buttonModelCreate[i]);
            }
            F_buttonModelCreate[0].Click += buttonModelCreateOne_Click;
            F_buttonModelCreate[1].Click += buttonModelCreateTwo_Click;
            F_buttonModelCreate[2].Click += buttonModelCreateThree_Click;
            F_buttonModelCreate[3].Click += buttonModelCreateFour_Click;
        }


        private void initializeButtonDestroyModel()
        {
            for (int i = 0; i < F_buttonModelDestroy.Length; ++i)
            {
                F_buttonModelDestroy[i] = new Button();
                F_buttonModelDestroy[i].Enabled = true;
                F_buttonModelDestroy[i].Visible = false;
                F_buttonModelDestroy[i].Size = new System.Drawing.Size(700, 25);
                F_buttonModelDestroy[i].Location = new Point(250, 160 * (i + 1) + 5);
                F_buttonModelDestroy[i].Text = "Press button to destroy model of conveyor";
                F_buttonModelDestroy[i].Font = new Font("Arial", 12, FontStyle.Bold);
                this.Controls.Add(F_buttonModelDestroy[i]);
            }
            F_buttonModelDestroy[0].Click += buttonModelDestroyOne_Click;
            F_buttonModelDestroy[1].Click += buttonModelDestroyTwo_Click;
            F_buttonModelDestroy[2].Click += buttonModelDestroyThree_Click;
            F_buttonModelDestroy[3].Click += buttonModelDestroyFour_Click;
        }


        private void initializeButtonCreateMechanic()
        {
            for (int i = 0; i < F_buttonMechanicCreate.Length; ++i)
            {
                F_buttonMechanicCreate[i] = new Button();
                F_buttonMechanicCreate[i].Enabled = true;
                F_buttonMechanicCreate[i].Visible = true;
                F_buttonMechanicCreate[i].Size = new System.Drawing.Size(100, 25);
                F_buttonMechanicCreate[i].Location = new Point(1000, 220 * (i + 1) - 25);
                F_buttonMechanicCreate[i].Text = "Recruit";
                F_buttonMechanicCreate[i].Font = new Font("Arial", 12, FontStyle.Bold);
                this.Controls.Add(F_buttonMechanicCreate[i]);
            }
            F_buttonMechanicCreate[0].Click += buttonMechanicCreateOne_Click;
            F_buttonMechanicCreate[1].Click += buttonMechanicCreateTwo_Click;
            F_buttonMechanicCreate[2].Click += buttonMechanicCreateThree_Click;
        }


        private void initializeButtonDestroyMechanic()
        {
            for (int i = 0; i < F_buttonMechanicDestroy.Length; ++i)
            {
                F_buttonMechanicDestroy[i] = new Button();
                F_buttonMechanicDestroy[i].Enabled = false;
                F_buttonMechanicDestroy[i].Visible = true;
                F_buttonMechanicDestroy[i].Size = new System.Drawing.Size(100, 25);
                F_buttonMechanicDestroy[i].Location = new Point(1105, 220 * (i + 1) - 25);
                F_buttonMechanicDestroy[i].Text = "Dismiss";
                F_buttonMechanicDestroy[i].Font = new Font("Arial", 12, FontStyle.Bold);
                this.Controls.Add(F_buttonMechanicDestroy[i]);
            }
            F_buttonMechanicDestroy[0].Click += buttonMechanicDestroyOne_Click;
            F_buttonMechanicDestroy[1].Click += buttonMechanicDestroyTwo_Click;
            F_buttonMechanicDestroy[2].Click += buttonMechanicDestroyThree_Click;
        }

        // Кнопки для создания новой модели конвеера
        private void buttonModelCreateOne_Click(object sender, EventArgs e)
        {
            F_buttonModelCreate[0].Visible = false;
            F_buttonModelDestroy[0].Visible = true;
            initializeThread(0);
        }


        private void buttonModelCreateTwo_Click(object sender, EventArgs e)
        {
            F_buttonModelCreate[1].Visible = false;
            F_buttonModelDestroy[1].Visible = true;
            initializeThread(1);
        }


        private void buttonModelCreateThree_Click(object sender, EventArgs e)
        {
            F_buttonModelCreate[2].Visible = false;
            F_buttonModelDestroy[2].Visible = true;
            initializeThread(2);
        }


        private void buttonModelCreateFour_Click(object sender, EventArgs e)
        {
            F_buttonModelCreate[3].Visible = false;
            F_buttonModelDestroy[3].Visible = true;
            initializeThread(3);
        }

        // Кнопки для уничтожения новой модели конвеера
        private void buttonModelDestroyOne_Click(object sender, EventArgs e)
        {
            F_buttonModelCreate[0].Visible = true;
            F_buttonModelDestroy[0].Visible = false;
            destructConveyor(0);
        }


        private void buttonModelDestroyTwo_Click(object sender, EventArgs e)
        {
            F_buttonModelCreate[1].Visible = true;
            F_buttonModelDestroy[1].Visible = false;
            destructConveyor(1);
        }


        private void buttonModelDestroyThree_Click(object sender, EventArgs e)
        {
            F_buttonModelCreate[2].Visible = true;
            F_buttonModelDestroy[2].Visible = false;
            destructConveyor(2);
        }


        private void buttonModelDestroyFour_Click(object sender, EventArgs e)
        {
            F_buttonModelCreate[3].Visible = true;
            F_buttonModelDestroy[3].Visible = false;
            destructConveyor(3);
        }

        // Кнопки для найма механика
        private void buttonMechanicCreateOne_Click(object sender, EventArgs e)
        {
            F_buttonMechanicCreate[0].Enabled = false;
            F_buttonMechanicDestroy[0].Enabled = true;
            initializeMechanic(0);
            PaintMechanic();
        }


        private void buttonMechanicCreateTwo_Click(object sender, EventArgs e)
        {
            F_buttonMechanicCreate[1].Enabled = false;
            F_buttonMechanicDestroy[1].Enabled = true;
            initializeMechanic(1);
            PaintMechanic();
        }


        private void buttonMechanicCreateThree_Click(object sender, EventArgs e)
        {
            F_buttonMechanicCreate[2].Enabled = false;
            F_buttonMechanicDestroy[2].Enabled = true;
            initializeMechanic(2);
            PaintMechanic();
        }

        // Кнопки для увольнение механика
        private void buttonMechanicDestroyOne_Click(object sender, EventArgs e)
        {
            F_buttonMechanicCreate[0].Enabled = true;
            F_buttonMechanicDestroy[0].Enabled = false;
            destructMechanic(0);
            PaintMechanic();
        }


        private void buttonMechanicDestroyTwo_Click(object sender, EventArgs e)
        {
            F_buttonMechanicCreate[1].Enabled = true;
            F_buttonMechanicDestroy[1].Enabled = false;
            destructMechanic(1);
            PaintMechanic();
        }


        private void buttonMechanicDestroyThree_Click(object sender, EventArgs e)
        {
            F_buttonMechanicCreate[2].Enabled = true;
            F_buttonMechanicDestroy[2].Enabled = false;
            destructMechanic(2);
            PaintMechanic();
        }
    }
}
