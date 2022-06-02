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

        Thread[] F_thread;
        bool[] F_threadStatus;
        object lockerConveyors;
        object lockerLoaders;
        private int F_iBusyThread { get; set; }

        private Graphics[] F_graphicsConveyors;
        private Graphics[] F_graphicsParts;
        private Graphics[] F_graphicsLoaders;

        private Bitmap conveyorImage;
        private Bitmap loaderImage;
        private Bitmap clearLoaderImage;
        private Bitmap partsImage;
        private Bitmap clearPartsImage;

        private Models.ChiefEngineer chiefmech;
        private Models.JuniorMechanic junmech;
        private Models.SeniorMechanic senmech;

        Button[] buttonModelCreate;
        Button[] buttonModelDestroy;


        private void initializeDate()
        {
            F_ccConveyorsMashine = new Controller.ConveyorsController[Program.F_iThreadNumber];
            F_lclLoadersMashine = new Controller.LoaderController[Program.F_iThreadNumber];
            F_thread = new Thread[Program.F_iThreadNumber];
            F_threadStatus = new bool[Program.F_iThreadNumber];
            lockerConveyors = new object();
            lockerLoaders = new object();
            F_iBusyThread = -1;
            F_graphicsConveyors = new Graphics[Program.F_iThreadNumber];
            F_graphicsParts = new Graphics[Program.F_iThreadNumber];
            F_graphicsLoaders = new Graphics[Program.F_iThreadNumber];
            buttonModelCreate = new Button[Program.F_iThreadNumber];
            buttonModelDestroy = new Button[Program.F_iThreadNumber];
            chiefmech = new Models.ChiefEngineer();
            junmech = new Models.JuniorMechanic();
            senmech = new Models.SeniorMechanic();
        }


        private void initializeImage()
        {
            conveyorImage = new Bitmap(@"../../Resources/conveyor.png");
            loaderImage = new Bitmap(@"../../Resources/loader.png");
            clearLoaderImage = new Bitmap(@"../../Resources/clearloader.png");
            partsImage = new Bitmap(@"../../Resources/part.png");
            clearPartsImage = new Bitmap(@"../../Resources/clearpart.png");
        }


        public ConveyorsForm()
        {
            InitializeComponent();
        }


        private void ConveyorsForm_Load(object sender, EventArgs e)
        {
            this.Size = new System.Drawing.Size(1280, 720);
            initializeDate();
            initializeImage();
            //initializeMechanic;
            //initTimer();
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
                F_iBusyThread = -1;
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


        public void loadTick(object obj)
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


        public void conveyorsTick(object obj)
        {
            int numThread = (int)obj;

            lock (lockerConveyors)
            {
                F_ccConveyorsMashine[numThread].conveyorOperation();
                F_ccConveyorsMashine[numThread].conveyorIsBroken();
                paintParts(numThread);
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
            F_graphicsConveyors[numThread].DrawImage(conveyorImage, 250, 160 * numThread + 30);
        }


        private void paintLoader(int numThread)
        {
            lock (lockerLoaders)
            {

                F_graphicsLoaders[numThread].DrawImage(clearLoaderImage, 0, 0);
                F_graphicsLoaders[numThread].DrawImage(loaderImage,
                    F_lclLoadersMashine[numThread].LC_lLoader.L_pbLoader.P_iPosX,
                    F_lclLoadersMashine[numThread].LC_lLoader.L_pbLoader.P_iPosY);
            }
        }



        private void initTimer()
        {
            System.Windows.Forms.Timer timerMechanic = new System.Windows.Forms.Timer();
            timerMechanic.Interval = 1000; // 1000 миллисекунд
            timerMechanic.Enabled = true;
            timerMechanic.Tick += timerMechanic_Tick;
        }


        private void initializeMechanic()
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


        private void initializeButton()
        {
            for (int i = 0; i < buttonModelCreate.Length; ++i)
            {
                buttonModelCreate[i] = new Button();
                buttonModelCreate[i].Enabled = true;
                buttonModelCreate[i].Visible = true;
                buttonModelCreate[i].Size = new System.Drawing.Size(700, 150);
                buttonModelCreate[i].Location = new Point(250, 160 * i + 30);
                buttonModelCreate[i].Text = "Press button to create new model of conveyor";
                buttonModelCreate[i].Font = new Font("Arial", 24, FontStyle.Bold);
                this.Controls.Add(buttonModelCreate[i]);
            }
            buttonModelCreate[0].Click += buttonModelCreateOne_Click;
            buttonModelCreate[1].Click += buttonModelCreateTwo_Click;
            buttonModelCreate[2].Click += buttonModelCreateThree_Click;
            buttonModelCreate[3].Click += buttonModelCreateFour_Click;
            for (int i = 0; i < buttonModelDestroy.Length; ++i)
            {
                buttonModelDestroy[i] = new Button();
                buttonModelDestroy[i].Enabled = true;
                buttonModelDestroy[i].Visible = false;
                buttonModelDestroy[i].Size = new System.Drawing.Size(700, 25);
                buttonModelDestroy[i].Location = new Point(250, 160 * (i + 1) + 5);
                buttonModelDestroy[i].Text = "Press button to destroy model of conveyor";
                buttonModelDestroy[i].Font = new Font("Arial", 12, FontStyle.Bold);
                this.Controls.Add(buttonModelDestroy[i]);
            }
            buttonModelDestroy[0].Click += buttonModelDestroyOne_Click;
            buttonModelDestroy[1].Click += buttonModelDestroyTwo_Click;
            buttonModelDestroy[2].Click += buttonModelDestroyThree_Click;
            buttonModelDestroy[3].Click += buttonModelDestroyFour_Click;
        }

        // Кнопки для создания новой модели конвеера
        void buttonModelCreateOne_Click(object sender, EventArgs e)
        {
            buttonModelCreate[0].Visible = false;
            buttonModelDestroy[0].Visible = true;
            initializeThread(0);
        }


        void buttonModelCreateTwo_Click(object sender, EventArgs e)
        {
            buttonModelCreate[1].Visible = false;
            buttonModelDestroy[1].Visible = true;
            initializeThread(1);
        }


        void buttonModelCreateThree_Click(object sender, EventArgs e)
        {
            buttonModelCreate[2].Visible = false;
            buttonModelDestroy[2].Visible = true;
            initializeThread(2);
        }


        void buttonModelCreateFour_Click(object sender, EventArgs e)
        {
            buttonModelCreate[3].Visible = false;
            buttonModelDestroy[3].Visible = true;
            initializeThread(3);
        }

        // Кнопки для уничтожения новой модели конвеера
        void buttonModelDestroyOne_Click(object sender, EventArgs e)
        {
            buttonModelCreate[0].Visible = true;
            buttonModelDestroy[0].Visible = false;
            destructConveyor(0);
        }


        void buttonModelDestroyTwo_Click(object sender, EventArgs e)
        {
            buttonModelCreate[1].Visible = true;
            buttonModelDestroy[1].Visible = false;
            destructConveyor(1);
        }


        void buttonModelDestroyThree_Click(object sender, EventArgs e)
        {
            buttonModelCreate[2].Visible = true;
            buttonModelDestroy[2].Visible = false;
            destructConveyor(2);
        }


        void buttonModelDestroyFour_Click(object sender, EventArgs e)
        {
            buttonModelCreate[3].Visible = true;
            buttonModelDestroy[3].Visible = false;
            destructConveyor(3);
        }
    }
}
