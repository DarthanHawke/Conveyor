using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.Drawing;


namespace Conveyor.Models
{
    // Данный класс имеет возможность создавать объект, способный ремонтировать конвеер.
    // При этом, сразу же при начале ремонтных работ конвеер возобнавляет работу
    class SeniorMechanic : Mechanics
    {
        public PictureBox SM_pbSenMech = new PictureBox();
        public int SM_iRepairSpeed { get; set; }    // Скорость починки
        public int SM_iProgress { get; set; }   // Прогресс починки
        public bool CE_bBusyness { get; set; }  // True - свободен, False- занят


        public void initializeMechanic()
        {
            SM_pbSenMech.Name = "senmech";
            SM_pbSenMech.Image = Image.FromFile(@"../../Resources/seniormechanic.png");
            SM_pbSenMech.Size = new System.Drawing.Size(220, 180);
            SM_pbSenMech.Location = new Point(1000, 250);
            SM_pbSenMech.SizeMode = PictureBoxSizeMode.Zoom;
            SM_iRepairSpeed = 15;
            SM_iProgress = 0;
            CE_bBusyness = true;
        }


        public void repairLoader(ref Models.Conveyors CC_cConveyor)
        {
            SM_pbSenMech.Location = new Point(CC_cConveyor.C_pbConveer.Location.X + 80, CC_cConveyor.C_pbConveer.Location.Y);
            if (SM_iProgress < Conveyors.C_iHitbox)
            {
                SM_iProgress += SM_iRepairSpeed;
            }
            else
            {
                SM_pbSenMech.Location = new Point(1000, 250);
                CC_cConveyor.C_bWorkStatus = true;
                SM_iProgress = 0;
            }
        }


        public void controlRepair(ref Models.Conveyors CC_cConveyor)
        {
            if (CC_cConveyor.C_bWorkStatus == false)
            {
                repairLoader(ref CC_cConveyor);
            }
        }
    }
}
