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
    // При этом, сразу же при начале ремонтных работ конвеер возобнавляет работу
    class SeniorMechanic : Models.Mechanics
    {
        public Models.Parts SM_pbSenMech = new Models.Parts();
        public int SM_iRepairSpeed { get; set; }    // Скорость починки
        public int SM_iProgress { get; set; }   // Прогресс починки
        public bool CE_bBusyness { get; set; }  // True - свободен, False- занят


        public SeniorMechanic()
        {
            initializeMechanic();
        }


        public void initializeMechanic()
        {
            SM_pbSenMech.Name = "senmech";
            SM_pbSenMech.P_iPosX = 1100;
            SM_pbSenMech.P_iPosY = 280;
            SM_iRepairSpeed = 7;
            SM_iProgress = 0;
            CE_bBusyness = true;
        }


        public void repairLoader(ref Models.Conveyors CC_cConveyor)
        {
            SM_pbSenMech.P_iPosX = CC_cConveyor.C_pbConveer.P_iPosX + 700;
            SM_pbSenMech.P_iPosY = CC_cConveyor.C_pbConveer.P_iPosY;
            if (SM_iProgress < Models.Conveyors.C_iHitbox)
            {
                SM_iProgress += SM_iRepairSpeed;
            }
            else
            {
                SM_pbSenMech.P_iPosX = 1100;
                SM_pbSenMech.P_iPosY = 280;
                CE_bBusyness = true;
                CC_cConveyor.C_bWorkStatus = true;
                CC_cConveyor.C_bRepairStatus = false;
                SM_iProgress = 0;
            }
        }


        public void controlRepair(ref Models.Conveyors CC_cConveyor)
        {
            CE_bBusyness = false;
            CC_cConveyor.C_bRepairStatus = true;
            if (CC_cConveyor.C_bWorkStatus == false)
            {
                repairLoader(ref CC_cConveyor);
            }
        }
    }
}
