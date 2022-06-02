using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.Drawing;


namespace Conveyor.Controller
{
    // Данный класс имеет возможность создавать объект, способный ремонтировать конвеер.
    // При ремонте, конвеер не деспособен до починки
    class JuniorMechanic : Models.Mechanics
    {
        public Models.Parts JM_pbJunMech = new Models.Parts();
        public int JM_iRepairSpeed { get; set; }    // Скорость починки
        public int JM_iProgress { get; set; }   // Прогресс починки
        public bool CE_bBusyness { get; set; }  // True - свободен, False- занят


        public JuniorMechanic()
        {
            initializeMechanic();
        }


        public void initializeMechanic()
        {
            JM_pbJunMech.Name = "junmech";
            JM_pbJunMech.P_iPosX = 1100;
            JM_pbJunMech.P_iPosY = 500;
            JM_iRepairSpeed = 5;
            JM_iProgress = 0;
            CE_bBusyness = true;
        }


        public void repairLoader(ref Models.Conveyors CC_cConveyor)
        {
            JM_pbJunMech.P_iPosX = CC_cConveyor.C_pbConveer.P_iPosX + 700;
            JM_pbJunMech.P_iPosY = CC_cConveyor.C_pbConveer.P_iPosY;

            if (JM_iProgress < Models.Conveyors.C_iHitbox)
            {
                JM_iProgress += JM_iRepairSpeed;
            }
            else
            {
                JM_pbJunMech.P_iPosX = 1100;
                JM_pbJunMech.P_iPosY = 500;
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
