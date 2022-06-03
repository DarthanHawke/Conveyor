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

        private Thread[] F_thread;
        private bool[] F_threadStatus;
        private object F_oLockerConveyors;
        private object F_oLockerLoaders;
        private object F_oLockerMechanic;
        private int F_iBusyThread { get; set; }
        private int[] F_iLoaderMechanic;
        private int countMechanic { get; set; }

        private Graphics[] F_graphicsConveyors;
        private Graphics[] F_graphicsParts;
        private Graphics[] F_graphicsLoaders;
        private Graphics F_graphicsMechanic;
        private PictureBox[] F_pbDisableMichanics;
        private Label[] F_lNameMechanics;
        private Label[] F_lParametersMechanics;

        private Button[] F_buttonModelCreate;
        private Button[] F_buttonModelDestroy;
        private Button[] F_buttonMechanicCreate;
        private Button[] F_buttonMechanicDestroy;
        private Button F_buttonInfo;
        private Button F_buttonTask;

        private Bitmap conveyorImage;
        private Bitmap loaderImage;
        private Bitmap clearLoaderImage;
        private Bitmap partsImage;
        private Bitmap clearPartsImage;
        private Bitmap fireImage;
        private Bitmap chiefmechImage;
        private Bitmap senmechImage;
        private Bitmap junmechImage;
        private Bitmap clearMechImage;

        // Инициализируем необходимые для модели данные
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
            F_iLoaderMechanic = new int[3];
            countMechanic = 0;

            F_graphicsConveyors = new Graphics[Program.F_iThreadNumber];
            F_graphicsParts = new Graphics[Program.F_iThreadNumber];
            F_graphicsLoaders = new Graphics[Program.F_iThreadNumber];
            F_graphicsMechanic = CreateGraphics();
            F_pbDisableMichanics = new PictureBox[3];
            F_lNameMechanics = new Label[3];
            F_lParametersMechanics = new Label[3];

            F_buttonModelCreate = new Button[Program.F_iThreadNumber];
            F_buttonModelDestroy = new Button[Program.F_iThreadNumber];
            F_buttonMechanicCreate = new Button[3];
            F_buttonMechanicDestroy = new Button[3];
            F_buttonInfo = new Button();
            F_buttonTask = new Button();
        }

        // Инициализируем исходные изображения
        private void initializeImage()
        {
            conveyorImage = new Bitmap(@"../../Resources/conveyor.png");
            loaderImage = new Bitmap(@"../../Resources/loader.png");
            clearLoaderImage = new Bitmap(@"../../Resources/clearloader.png");
            partsImage = new Bitmap(@"../../Resources/part.png");
            clearPartsImage = new Bitmap(@"../../Resources/clearpart.png");
            fireImage = new Bitmap(@"../../Resources/fire.png");
            chiefmechImage = new Bitmap(@"../../Resources/chiefengineerdisable.png");
            senmechImage = new Bitmap(@"../../Resources/seniormechanicdisable.png");
            junmechImage = new Bitmap(@"../../Resources/juniormechanicdisable.png");
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
            initializeImageMechanic();
            initializeAdditionalButton();
        }

        // Инициализируем и запускаем потоки
        private void initializeThread(int numThread)
        {
            F_thread[numThread] = new Thread((obj) => startConveyors(obj));

            F_thread[numThread].Start(numThread);
        }

        // При нажатии кнопки, удаляющей конвеер, получаем сигнал - F_threadStatus = false,
        // который приводит к завершению потока. Далее ожидаем завершение потока и обнуляем выбранный конвеер
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

        // Запускаем конвеер
        private void startConveyors(object obj)
        {
            // Инициализируем данные для нового конвеера
            int numThread = (int)obj;
            F_threadStatus[numThread] = true;
            F_ccConveyorsMashine[numThread] = new Controller.ConveyorsController(160 * numThread + 30);
            F_lclLoadersMashine[numThread] = new Controller.LoaderController(160 * numThread + 30);
            F_graphicsConveyors[numThread] = CreateGraphics();
            F_graphicsLoaders[numThread] = CreateGraphics();
            F_graphicsParts[numThread] = CreateGraphics();
            paintConveyor(numThread);

            // Вешаем таймер на погрузчик внутри потока (Чтобы обращаться к данным, инициализированным конкретно в этом потоке.
            // Если бы повесили таймер вне потока, пришлось бы ассинхронно обращаться к данным, например через invoke.
            TimerCallback tmLoad = new TimerCallback(loadTick);
            System.Threading.Timer timerLoad = new System.Threading.Timer(tmLoad, numThread, 0, 700);

            // Вешаем таймер на механиков
            TimerCallback tmMechanic = new TimerCallback(mechanicTick);
            System.Threading.Timer timerMechanic = new System.Threading.Timer(tmMechanic, null, 0, 1000);

            // Вешаем таймер на сам конвеер
            TimerCallback tmConveyors = new TimerCallback(conveyorsTick);
            System.Threading.Timer timerConveyors = new System.Threading.Timer(tmConveyors, numThread, 0, 100);

            // Бесконечный цикл, пока не заврешится поток принудительно, или не будет получен сигнал с кнопки удаления конвеера
            while (F_thread[numThread].IsAlive && F_threadStatus[numThread] == true) { }
            AutoResetEvent waitHandlerLoad = new AutoResetEvent(false);
            AutoResetEvent waitHandlerConveyors = new AutoResetEvent(false);
            timerLoad.Dispose(waitHandlerLoad);
            timerConveyors.Dispose(waitHandlerConveyors);
            waitHandlerLoad.WaitOne();
            // Останавливаем все таймеры и дожидаемся их остановки, что бы не отчистить данные конвеера, во время тика таймера
            waitHandlerConveyors.WaitOne();
        }

        // Действие погрузчика
        private void loadTick(object obj)
        {
            int numThread = (int)obj;

            // Если загрзчик занят определённым конвеером(по индексу потока), и именно
            // для этого конвеера вызван этот метод или погрузщик не занят не занят(-1), то
            if (numThread == F_iBusyThread || F_iBusyThread == -1)
            {
                // Закрепляем погрузчик за конвеером
                F_iBusyThread = numThread;
                // Грузим детали
                F_lclLoadersMashine[numThread].controlLoad(F_ccConveyorsMashine[numThread].CC_cConveyor);
                // Отрисовываем погрузчик рядом с нужным конвеером
                paintLoader(numThread);
                bool checkBusy = true;
                foreach (var LoadMashine in F_lclLoadersMashine)
                {
                    if (LoadMashine != null)
                    {
                        checkBusy = checkBusy && !LoadMashine.LC_lLoader.L_bLoading;
                    }
                }
                // Если погрузчик закончил погрузку, освобожаем для других конвееров
                if (checkBusy == true)
                {
                    F_iBusyThread = -1;
                }
            }
        }

        // Действие конвеера
        private void conveyorsTick(object obj)
        {
            int numThread = (int)obj;

            lock (F_oLockerConveyors)
            {
                if (F_ccConveyorsMashine[numThread].CC_cConveyor.C_bWorkStatus)
                {
                    // Передвигаем детальки
                    F_ccConveyorsMashine[numThread].conveyorOperation();
                    // Проверяем на исправность
                    F_ccConveyorsMashine[numThread].conveyorIsBroken();
                    // Отрисовываем детали по обновлённым данным
                    paintParts(numThread);
                }
            }
        }

        // Рисуем детальки
        private void paintParts(int numThread)
        {
            if (F_ccConveyorsMashine[numThread].CC_cConveyor.C_bWorkStatus == true)
            {
                // Отчищает детали со старыми координатами
                F_graphicsParts[numThread].DrawImage(clearPartsImage, 250, 160 * numThread + 30);
            }
            else
            {
                // Отрисовывает поломку конвеера
                F_graphicsParts[numThread].DrawImage(fireImage, 250, 160 * numThread + 30);
            }
            if (F_ccConveyorsMashine[numThread].CC_cConveyor.C_qConveyor.Count != 0)
            {
                foreach (var part in F_ccConveyorsMashine[numThread].CC_cConveyor.C_qConveyor)
                {
                    F_graphicsParts[numThread].DrawImage(partsImage, part.P_iPosX, part.P_iPosY);
                }
            }
        }

        // Рисуем конвеер
        private void paintConveyor(int numThread)
        {
            F_graphicsConveyors[numThread].DrawImage(conveyorImage,
                F_ccConveyorsMashine[numThread].CC_cConveyor.C_pbConveer.P_iPosX,
                F_ccConveyorsMashine[numThread].CC_cConveyor.C_pbConveer.P_iPosY);
        }

        // Рисуем загрузчик
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

        // Инициализируем механиков
        private void initializeMechanic(int number)
        {
            switch (number)
            {
                case 0:
                    F_ceChiefmech = new Controller.ChiefEngineer();
                    F_ceChiefmech.chiefOnField(F_smSenmech, F_jmJunmech);
                    chiefmechImage = new Bitmap(@"../../Resources/chiefengineer.png");
                    F_lParametersMechanics[0].Text = "Repair speed = " + F_ceChiefmech.CE_iRepairSpeed.ToString();
                    if (F_smSenmech != null)
                    {
                        F_lParametersMechanics[1].Text = "Repair speed = " + F_smSenmech.SM_iRepairSpeed.ToString();
                    }
                    if (F_jmJunmech != null)
                    {
                        F_lParametersMechanics[2].Text = "Repair speed = " + F_jmJunmech.JM_iRepairSpeed.ToString();
                    }
                    break;
                case 1:
                    F_smSenmech = new Controller.SeniorMechanic();
                    if (F_ceChiefmech != null)
                    {
                        F_ceChiefmech.chiefOnField(F_smSenmech, null);
                    }
                    senmechImage = new Bitmap(@"../../Resources/seniormechanic.png");
                    F_lParametersMechanics[1].Text = "Repair speed = " + F_smSenmech.SM_iRepairSpeed.ToString();
                    break;
                case 2:
                    F_jmJunmech = new Controller.JuniorMechanic();
                    if (F_ceChiefmech != null)
                    {
                        F_ceChiefmech.chiefOnField(null, F_jmJunmech);
                    }
                    junmechImage = new Bitmap(@"../../Resources/juniormechanic.png");
                    F_lParametersMechanics[2].Text = "Repair speed = " + F_jmJunmech.JM_iRepairSpeed.ToString();
                    break;
            }
        }

        // Обнуляем механиков
        private void destructMechanic(int number)
        {
            switch (number)
            {
                case 0:
                    while (!F_ceChiefmech.CE_bBusyness) { }
                    F_ceChiefmech.chiefOffField(F_smSenmech, F_jmJunmech);
                    F_ceChiefmech = null;
                    chiefmechImage = new Bitmap(@"../../Resources/chiefengineerdisable.png");
                    if (F_smSenmech != null)
                    {
                        F_lParametersMechanics[1].Text = "Repair speed = " + F_smSenmech.SM_iRepairSpeed.ToString();
                    }
                    if (F_jmJunmech != null)
                    {
                        F_lParametersMechanics[2].Text = "Repair speed = " + F_jmJunmech.JM_iRepairSpeed.ToString();
                    }
                    break;
                case 1:
                    while (!F_smSenmech.CE_bBusyness) { }
                    if (F_ceChiefmech != null)
                    {
                        F_ceChiefmech.chiefOffField(F_smSenmech, null);
                    }
                    F_smSenmech = null;
                    senmechImage = new Bitmap(@"../../Resources/seniormechanicdisable.png");
                    break;
                case 2:
                    while (!F_jmJunmech.CE_bBusyness) { }
                    if (F_ceChiefmech != null)
                    {
                        F_ceChiefmech.chiefOffField(null, F_jmJunmech);
                    }
                    F_jmJunmech = null;
                    junmechImage = new Bitmap(@"../../Resources/juniormechanicdisable.png");
                    break;
            }
        }

        // Действие механиков
        private void mechanicTick(object sender)
        {
            // Ищет свободного механика и занимаем его за нужным конвеером
            mechanicSearch();
            if (F_ceChiefmech != null)
            {
                // Если механик занят, то продолжаем чинить, как починит, сменит статус на свободного
                if (!F_ceChiefmech.CE_bBusyness)
                {
                    F_ceChiefmech.controlRepair(ref F_ccConveyorsMashine[F_iLoaderMechanic[0]].CC_cConveyor);
                }
            }
            if (F_smSenmech != null)
            {
                if (!F_smSenmech.CE_bBusyness)
                {
                    F_smSenmech.controlRepair(ref F_ccConveyorsMashine[F_iLoaderMechanic[1]].CC_cConveyor);
                }
            }
            if (F_jmJunmech != null)
            {
                if (!F_jmJunmech.CE_bBusyness)
                {
                    F_jmJunmech.controlRepair(ref F_ccConveyorsMashine[F_iLoaderMechanic[2]].CC_cConveyor);
                }
            }
            PaintMechanic();
        }

        // Поиск свободных механиков
        private void mechanicSearch()
        {
            bool findMechanic = false;
            for (int i = 0; i < Program.F_iThreadNumber; ++i)
            {
                if (F_ccConveyorsMashine[i] != null)
                {
                    if (!F_ccConveyorsMashine[i].CC_cConveyor.C_bWorkStatus
                        && !F_ccConveyorsMashine[i].CC_cConveyor.C_bRepairStatus)
                    {
                        if (F_jmJunmech != null && findMechanic == false)
                        {
                            if (F_jmJunmech.CE_bBusyness)
                            {
                                // есди свободен, помечаем что нашли, чтоб выйти из условия,
                                // помечаем, что механик занаят, и запомниаем, какой конвеер он ремонтирует
                                findMechanic = true;
                                F_jmJunmech.CE_bBusyness = false;
                                F_iLoaderMechanic[2] = i;
                            }
                        }
                        if (F_smSenmech != null && findMechanic == false)
                        {
                            if (F_smSenmech.CE_bBusyness)
                            {
                                findMechanic = true;
                                F_smSenmech.CE_bBusyness = false;
                                F_iLoaderMechanic[1] = i;
                            }
                        }
                        if (F_ceChiefmech != null && findMechanic == false)
                        {
                            if (F_ceChiefmech.CE_bBusyness)
                            {
                                findMechanic = true;
                                F_ceChiefmech.CE_bBusyness = false;
                                F_iLoaderMechanic[0] = i;
                            }
                        }
                    }
                }
            }
        }

        // Отрисовываем механиков
        private void PaintMechanic()
        {
            lock (F_oLockerMechanic)
            {
                F_graphicsMechanic.DrawImage(clearMechImage, 950, 0);
                if (F_ceChiefmech != null)
                {
                    F_graphicsMechanic.DrawImage(chiefmechImage, F_ceChiefmech.CE_pbChiefMech.P_iPosX, F_ceChiefmech.CE_pbChiefMech.P_iPosY);
                }
                if (F_smSenmech != null)
                {
                    F_graphicsMechanic.DrawImage(senmechImage, F_smSenmech.SM_pbSenMech.P_iPosX, F_smSenmech.SM_pbSenMech.P_iPosY);
                }
                if (F_jmJunmech != null)
                {
                    F_graphicsMechanic.DrawImage(junmechImage, F_jmJunmech.JM_pbJunMech.P_iPosX, F_jmJunmech.JM_pbJunMech.P_iPosY);
                }
            }
        }

        #region Button Form Code
        // Здесь содержатся все доступные кнопки программы.
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
                F_buttonModelCreate[i].Size = new Size(700, 150);
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
                F_buttonModelDestroy[i].Size = new Size(700, 25);
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
                F_buttonMechanicCreate[i].Size = new Size(80, 25);
                F_buttonMechanicCreate[i].Location = new Point(1090, 220 * (i + 1) - 25);
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
                F_buttonMechanicDestroy[i].Size = new Size(80, 25);
                F_buttonMechanicDestroy[i].Location = new Point(1170, 220 * (i + 1) - 25);
                F_buttonMechanicDestroy[i].Text = "Dismiss";
                F_buttonMechanicDestroy[i].Font = new Font("Arial", 12, FontStyle.Bold);
                this.Controls.Add(F_buttonMechanicDestroy[i]);
            }
            F_buttonMechanicDestroy[0].Click += buttonMechanicDestroyOne_Click;
            F_buttonMechanicDestroy[1].Click += buttonMechanicDestroyTwo_Click;
            F_buttonMechanicDestroy[2].Click += buttonMechanicDestroyThree_Click;
        }


        private void initializeImageMechanic()
        {
            for (int i = 0; i < F_pbDisableMichanics.Length; ++i)
            {
                F_pbDisableMichanics[i] = new PictureBox();
                F_pbDisableMichanics[i].Visible = true;
                F_pbDisableMichanics[i].Size = new Size(114, 134);
                F_pbDisableMichanics[i].SizeMode = PictureBoxSizeMode.Zoom;
                this.Controls.Add(F_pbDisableMichanics[i]);
                F_lNameMechanics[i] = new Label();
                F_lNameMechanics[i].Visible = true;
                F_lNameMechanics[i].Size = new Size(170, 20);
                F_lNameMechanics[i].Font = new Font("Arial", 13, FontStyle.Bold);
                this.Controls.Add(F_lNameMechanics[i]);
                F_lParametersMechanics[i] = new Label();
                F_lParametersMechanics[i].Visible = false;
                F_lParametersMechanics[i].Size = new Size(170, 20);
                F_lParametersMechanics[i].Font = new Font("Arial", 11, FontStyle.Regular);
                this.Controls.Add(F_lParametersMechanics[i]);
            }
            F_pbDisableMichanics[0].Image = new Bitmap(@"../../Resources/chiefengineerdisable.png");
            F_lNameMechanics[0].Text = "Chief Engineer";
            F_pbDisableMichanics[0].Location = new Point(1100, 60);
            F_lNameMechanics[0].Location = new Point(1100, 20);
            F_lParametersMechanics[0].Location = new Point(1100, 40);
            F_pbDisableMichanics[1].Image = new Bitmap(@"../../Resources/seniormechanicdisable.png");
            F_lNameMechanics[1].Text = "Senior Mechanic";
            F_pbDisableMichanics[1].Location = new Point(1100, 280);
            F_lNameMechanics[1].Location = new Point(1100, 240);
            F_lParametersMechanics[1].Location = new Point(1100, 260);
            F_pbDisableMichanics[2].Image = new Bitmap(@"../../Resources/juniormechanicdisable.png");
            F_lNameMechanics[2].Text = "Junior Mechanic";
            F_pbDisableMichanics[2].Location = new Point(1100, 500);
            F_lNameMechanics[2].Location = new Point(1100, 460);
            F_lParametersMechanics[2].Location = new Point(1100, 480);
        }


        private void initializeAdditionalButton()
        {
            F_buttonInfo.Enabled = true;
            F_buttonInfo.Visible = true;
            F_buttonInfo.Size = new Size(60, 30);
            F_buttonInfo.Location = new Point(0, 650);
            F_buttonInfo.Text = "Info";
            F_buttonInfo.Font = new Font("Arial", 12, FontStyle.Regular);
            F_buttonInfo.Click += buttonInfo_Click;
            this.Controls.Add(F_buttonInfo);

            F_buttonTask.Enabled = true;
            F_buttonTask.Visible = true;
            F_buttonTask.Size = new Size(60, 30);
            F_buttonTask.Location = new Point(60, 650);
            F_buttonTask.Text = "Task";
            F_buttonTask.Font = new Font("Arial", 12, FontStyle.Regular);
            F_buttonTask.Click += buttonTask_Click;
            this.Controls.Add(F_buttonTask);
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
            if (countMechanic < 2)
            {
                ++countMechanic;
                F_pbDisableMichanics[0].Visible = false;
                F_lParametersMechanics[0].Visible = true;
                F_buttonMechanicCreate[0].Enabled = false;
                F_buttonMechanicDestroy[0].Enabled = true;
                initializeMechanic(0);
                PaintMechanic();
            }
            else
            {
                messageNoMoreTwo();
            }
        }


        private void buttonMechanicCreateTwo_Click(object sender, EventArgs e)
        {
            if (countMechanic < 2)
            {
                ++countMechanic;
                F_pbDisableMichanics[1].Visible = false;
                F_lParametersMechanics[1].Visible = true;
                F_buttonMechanicCreate[1].Enabled = false;
                F_buttonMechanicDestroy[1].Enabled = true;
                initializeMechanic(1);
                PaintMechanic();
            }
            else
            {
                messageNoMoreTwo();
            }
        }


        private void buttonMechanicCreateThree_Click(object sender, EventArgs e)
        {
            if (countMechanic < 2)
            {
                ++countMechanic;
                F_pbDisableMichanics[2].Visible = false;
                F_lParametersMechanics[2].Visible = true;
                F_buttonMechanicCreate[2].Enabled = false;
                F_buttonMechanicDestroy[2].Enabled = true;
                initializeMechanic(2);
                PaintMechanic();
            }
            else
            {
                messageNoMoreTwo();
            }
        }

        // Кнопки для увольнение механика
        private void buttonMechanicDestroyOne_Click(object sender, EventArgs e)
        {
            --countMechanic;
            F_pbDisableMichanics[0].Visible = true;
            F_lParametersMechanics[0].Visible = false;
            F_buttonMechanicCreate[0].Enabled = true;
            F_buttonMechanicDestroy[0].Enabled = false;
            destructMechanic(0);
            PaintMechanic();
        }


        private void buttonMechanicDestroyTwo_Click(object sender, EventArgs e)
        {
            --countMechanic;
            F_pbDisableMichanics[1].Visible = true;
            F_lParametersMechanics[1].Visible = false;
            F_buttonMechanicCreate[1].Enabled = true;
            F_buttonMechanicDestroy[1].Enabled = false;
            destructMechanic(1);
            PaintMechanic();
        }


        private void buttonMechanicDestroyThree_Click(object sender, EventArgs e)
        {
            --countMechanic;
            F_pbDisableMichanics[2].Visible = true;
            F_lParametersMechanics[2].Visible = false;
            F_buttonMechanicCreate[2].Enabled = true;
            F_buttonMechanicDestroy[2].Enabled = false;
            destructMechanic(2);
            PaintMechanic();
        }


        private void buttonInfo_Click(object sender, EventArgs e)
        {
            messageProgramInformation();
        }


        private void buttonTask_Click(object sender, EventArgs e)
        {
            messageTaskInformation();
        }


        private void messageNoMoreTwo()
        {
            MessageBox.Show(
                "Максимальное колличество механиков в модели не более двух. " +
                "Удалите одного из существующих механиков, чтобы нанять нового.",
                "Ошибка",
                MessageBoxButtons.OK,
                MessageBoxIcon.Warning,
                MessageBoxDefaultButton.Button1);
        }


        private void messageProgramInformation()
        {
            MessageBox.Show(
                "Данная программа моделирует процесс конвеера.\n" +
                "Погрузчик загружает товары на конвеер, а конвеерная линия двигает их.\n" +
                "Конвеер с некоторой вероятностью может сломаться. Для востановления конвеера" +
                "механик должен восстановить 100 единиц прочности конвеера.\n" +
                "Доступно на выбор три механика, наследуемых от одного интерфейса.\n" +
                "\nПервый механик - Chief Engineer имеет самую низкую скорость починки в 2 единицы в секунду," +
                "однако даёт бонус к скорости починки конвеера другим механикам." +
                "+10 единиц для SeniorMechanic и +17 единиц для JuniorMechanic.\n" +
                "\nВторой механик - SeniorMechanic имеет скорость ремонта 7 единиц в секунду.\n" +
                "\nТретий механик - JuniorMechanic имеет скорость ремонта 3 единицы в секунду.\n" +
                "\nНа форме можно разместить четыре модели конвеера. Под работу каждого конвеера" +
                "создаётся собственный поток. Таким образом четыре конвеера будут выполняться в четырёх разных потоках.",
                "Информация о программе",
                MessageBoxButtons.OK,
                MessageBoxIcon.Information,
                MessageBoxDefaultButton.Button1);
        }


        private void messageTaskInformation()
        {
            MessageBox.Show(
                "Зачание №5, 6. Вариант №19.\n" +
                "\n№5. В зависимости от задачи необходимо смоделировать ситуацию/процесс.\n" +
                "В каждой модели есть набор возможных ситуаций.\n" +
                "Для некоторых событий необходимо определить вероятность возникновения данного события.\n" +
                "Интерфейс необходимо реализовать, используя 3 и более классов.\n" +
                "\nДля решения задач необходимо использовать:\n" +
                "\n1. Делегаты / события.\n" + 
                "2. Многопоточность\n" +
                "3. Где необходимо рефлексию\n" +
                "\nНа форме должно быть динамическое изменение моделей – все должно двигаться.\n" +
                "Иметь возможность добавлять несколько моделей на форму.\n" +
                "\n19. Конвейер с деталями – смоделировать работу конвейера производства деталей.\n" +
                "Реализовать классы – Конвейер, Погрузчик, интерфейс – Механик.\n" +
                "События – в конвейере закончились материалы – погрузчик загружает новую партию,\n" +
                "конвейер сломался (с некоторой долей вероятности) – механик чинит конвейер.\n" +
                "\n№6. Доработать предыдущую задачу с использованием синхронизации потоков. " +
                "На форме должно быть не менее 4 моделей. " +
                "Ограничения накладываются на классы, которые реализуют интерфейсы. " +
                "Для 4 моделей должно быть 2 объекта данных  классов в сумме. " +
                "При возникновении какого-то события 1 из объектов «лочится» " +
                "и не доступен для использования в других моделях.",
                "Условие задачи",
                MessageBoxButtons.OK,
                MessageBoxIcon.Information,
                MessageBoxDefaultButton.Button1);
        }
        #endregion
    }
}
