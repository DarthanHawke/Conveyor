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
        public Models.Loaders LC_lLoader = new Models.Loaders();

        public void initializeLoaderController()
        {
            LC_lLoader.L_iLoadBatch = 0;
            LC_lLoader.L_bLoading = false;
        }


        public void initLoader()
        {
            LC_lLoader.L_pbLoader.Name = "loader";
            LC_lLoader.L_pbLoader.Image = Image.FromFile(@"../../Resources/loader.png");
            LC_lLoader.L_pbLoader.Size = new System.Drawing.Size(120, 180);
            LC_lLoader.L_pbLoader.Location = new Point(50, 230);
            LC_lLoader.L_pbLoader.SizeMode = PictureBoxSizeMode.Zoom;
        }

        // Процесс загрузки деталей погрузщиком
        public PictureBox loadParts()
        {
            PictureBox pParts = new PictureBox();
            pParts.Name = "part";
            pParts.Image = Image.FromFile(@"../../Resources/part.png");
            pParts.Location = new Point(325, 82);
            pParts.Size = new System.Drawing.Size(75, 37);
            pParts.SizeMode = PictureBoxSizeMode.Zoom;
            return pParts;
        }

        // Загрузка конвеера погрузщиком
        public void loadConveyor(ref Models.Conveyors CC_cConveyor)
        {
            if (LC_lLoader.L_iLoadBatch < Models.Loaders.L_iBatch)
            {
                LC_lLoader.L_pbLoader.Location = new Point(130, 30);
                ++LC_lLoader.L_iLoadBatch;
                CC_cConveyor.C_qReserve.Push(loadParts());
            }
            else
            {
                LC_lLoader.L_pbLoader.Location = new Point(50, 230);
                LC_lLoader.L_iLoadBatch = 0;
                LC_lLoader.L_bLoading = false;
            }
        }


        public void controlLoad(ref Models.Conveyors CC_cConveyor)
        {
            if (CC_cConveyor.C_qReserve.Count == 0 || LC_lLoader.L_bLoading == true)
            {
                LC_lLoader.L_bLoading = true;
                loadConveyor(ref CC_cConveyor);
            }
        }
    }
}
