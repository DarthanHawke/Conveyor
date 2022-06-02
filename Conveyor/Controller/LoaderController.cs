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
        private int CC_iStartY { get; set; }


        public LoaderController()
        {
            initializeLoaderController();
            initLoader();
        }


        public LoaderController(int startY)
        {
            initializeLoaderController(startY);
            initLoader();
        }


        public void initializeLoaderController()
        {
            LC_lLoader = new Models.Loaders();
            LC_lLoader.L_iLoadBatch = 0;
            LC_lLoader.L_bLoading = false;
            CC_iStartY = 0;
        }


        public void initializeLoaderController(int startY)
        {
            LC_lLoader = new Models.Loaders();
            LC_lLoader.L_iLoadBatch = 0;
            LC_lLoader.L_bLoading = false;
            CC_iStartY = startY;
        }


        public void initLoader()
        {
            LC_lLoader.L_pbLoader.P_iPosX = 50;
            LC_lLoader.L_pbLoader.P_iPosY = 230;
        }

        // Процесс загрузки деталей погрузщиком
        public Models.Parts loadParts()
        {
            Models.Parts newPart = new Models.Parts();
            newPart.Name = "part";
            newPart.P_iPosX = 325;
            newPart.P_iPosY = 65;
            return newPart;
        }

        // Загрузка конвеера погрузщиком
        public void loadConveyor(Models.Conveyors CC_cConveyor)
        {
            if (LC_lLoader.L_iLoadBatch < Models.Loaders.L_iBatch)
            {
                LC_lLoader.L_pbLoader.P_iPosX = 130;
                LC_lLoader.L_pbLoader.P_iPosY = CC_iStartY;
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
