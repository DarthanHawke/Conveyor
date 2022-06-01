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
    // При ремонте, конвеер не деспособен до починки
    class JuniorMechanic : Mechanics
    {
        public PictureBox JM_pbJunMech = new PictureBox();
        public int JM_iRepairSpeed { get; set; }    // Скорость починки
        public int JM_iProgress { get; set; }   // Прогресс починки
        public bool CE_bBusyness { get; set; }  // True - свободен, False- занят


        public void initializeMechanic()
        {
            JM_pbJunMech.Name = "junmech";
            JM_pbJunMech.Image = Image.FromFile(@"../../Resources/juniormechanic.png");
            JM_pbJunMech.Size = new System.Drawing.Size(120, 180);
            JM_pbJunMech.Location = new Point(1000, 500);
            JM_pbJunMech.SizeMode = PictureBoxSizeMode.Zoom;
            JM_iRepairSpeed = 5;
            JM_iProgress = 0;
            CE_bBusyness = true;
        }


        public void repairLoader(ref Models.Conveyors CC_cConveyor)
        {
            JM_pbJunMech.Location = new Point(CC_cConveyor.C_pbConveer.P_iPosX + 80, CC_cConveyor.C_pbConveer.P_iPosY);
            if (JM_iProgress < Conveyors.C_iHitbox)
            {
                JM_iProgress += JM_iRepairSpeed;
            }
            else
            {
                JM_pbJunMech.Location = new Point(1000, 500);
                CC_cConveyor.C_bWorkStatus = true;
                JM_iProgress = 0;
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
