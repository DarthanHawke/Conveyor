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
    // При этом, сразу же при начале ремонтных работ конвеер возобнавляет работу.
    // Более того, если данный объект класса добавлен в модель, то другие существующие механики получают бонус к скорости ремонта.
    class ChiefEngineer : Models.Mechanics
    {
        public Models.Parts CE_pbChiefMech = new Models.Parts();
        public int CE_iRepairSpeed { get; set; }    // Скорость починки
        public int CE_iProgress { get; set; }   // Прогресс починки
        public bool CE_bBusyness { get; set; }  // True - свободен, False- занят


        public ChiefEngineer()
        {
            initializeMechanic();
        }


        public void initializeMechanic()
        {
            CE_pbChiefMech.Name = "chiefmech";
            CE_pbChiefMech.P_iPosX = 1100;
            CE_pbChiefMech.P_iPosY = 50;
            CE_iRepairSpeed = 10;
            CE_iProgress = 0;
            CE_bBusyness = true;
        }


        public void chiefOnField(ChiefEngineer chiefmech = null, SeniorMechanic senmech = null, JuniorMechanic junmech = null)
        {
            chiefmech.CE_iRepairSpeed += 10;
            senmech.SM_iRepairSpeed += 15;
            junmech.JM_iRepairSpeed += 25;
        }


        public void chiefOffField(ChiefEngineer chiefmech = null, SeniorMechanic senmech = null, JuniorMechanic junmech = null)
        {
            chiefmech.CE_iRepairSpeed -= 10;
            senmech.SM_iRepairSpeed -= 15;
            junmech.JM_iRepairSpeed -= 25;
        }


        public void repairLoader(ref Models.Conveyors CC_cConveyor)
        {
            CE_pbChiefMech.P_iPosX = CC_cConveyor.C_pbConveer.P_iPosX + 700;
            CE_pbChiefMech.P_iPosY = CC_cConveyor.C_pbConveer.P_iPosY;
            if (CE_iProgress < Models.Conveyors.C_iHitbox)
            {
                CE_iProgress += CE_iRepairSpeed;
            }
            else
            {
                CE_pbChiefMech.P_iPosX = 1100;
                CE_pbChiefMech.P_iPosY = 0;
                CC_cConveyor.C_bWorkStatus = true;
                CE_iProgress = 0;
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
