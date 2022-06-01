using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.Drawing;


namespace Conveyor.Controller
{
    class LoaderController
    {
        public Models.Loaders LC_lLoader;

        public LoaderController()
        {
            initializeLoaderController();
            initLoader();
        }


        public void initializeLoaderController()
        {
            LC_lLoader = new Models.Loaders();
            LC_lLoader.L_iLoadBatch = 0;
            LC_lLoader.L_bLoading = false;
        }


        public void initLoader()
        {
            LC_lLoader.L_pbLoader.P_iPosX = 50;
            LC_lLoader.L_pbLoader.P_iPosY = 230;

            /*LC_lLoader.L_pbLoader.Name = "loader";
            LC_lLoader.L_pbLoader.Image = Image.FromFile(@"../../Resources/loader.png");
            LC_lLoader.L_pbLoader.Size = new System.Drawing.Size(120, 180);
            LC_lLoader.L_pbLoader.Location = new Point(50, 230);
            LC_lLoader.L_pbLoader.SizeMode = PictureBoxSizeMode.Zoom;*/
        }

        // Процесс загрузки деталей погрузщиком
        public Models.Parts loadParts()
        {
            Models.Parts newPart = new Models.Parts();
            newPart.Name = "part";
            newPart.P_iPosX = 325;
            newPart.P_iPosY = 65;

            /*LC_lLoader.L_pbParts.Name = "part";
            LC_lLoader.L_pbParts.Image = Image.FromFile(@"../../Resources/part.png");
            LC_lLoader.L_pbParts.Location = new Point(325, 82);
            LC_lLoader.L_pbParts.Size = new System.Drawing.Size(75, 37);
            LC_lLoader.L_pbParts.SizeMode = PictureBoxSizeMode.Zoom;*/
            return newPart;
        }

        // Загрузка конвеера погрузщиком
        public void loadConveyor(Models.Conveyors CC_cConveyor)
        {
            if (LC_lLoader.L_iLoadBatch < Models.Loaders.L_iBatch)
            {
                LC_lLoader.L_pbLoader.P_iPosX = 130;
                LC_lLoader.L_pbLoader.P_iPosY = 30;
                ++LC_lLoader.L_iLoadBatch;
                CC_cConveyor.C_qReserve.Push(loadParts());
            }
            else
            {
                LC_lLoader.L_pbLoader.P_iPosX = 50;
                LC_lLoader.L_pbLoader.P_iPosY = 230;
                LC_lLoader.L_iLoadBatch = 0;
                LC_lLoader.L_bLoading = false;
            }
        }


        public void controlLoad(Models.Conveyors CC_cConveyor)
        {
            if (CC_cConveyor.C_qReserve.Count == 0 || LC_lLoader.L_bLoading == true)
            {
                LC_lLoader.L_bLoading = true;
                loadConveyor(CC_cConveyor);
            }
        }
    }
}
