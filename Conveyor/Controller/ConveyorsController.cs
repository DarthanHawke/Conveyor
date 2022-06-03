using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.Drawing;


namespace Conveyor.Controller
{
    class ConveyorsController
    {
        public Models.Conveyors CC_cConveyor;
        private Random CC_rBreakdown = new Random();
        private int CC_iStartY { get; set; }

        public ConveyorsController()
        {
            initializeConveyorsController();
            initConveyor();
        }

        public ConveyorsController(int startY)
        {
            initializeConveyorsController(startY);
            initConveyor(startY);
        }

        private void initializeConveyorsController()
        {
            CC_cConveyor = new Models.Conveyors();
            CC_iStartY = 0;
            CC_cConveyor.C_bLoad = false;
            CC_cConveyor.C_bWorkStatus = true;
            CC_cConveyor.C_bRepairStatus = false;
        }

        private void initializeConveyorsController(int startY)
        {
            CC_cConveyor = new Models.Conveyors();
            CC_iStartY = startY;
            CC_cConveyor.C_bLoad = false;
            CC_cConveyor.C_bWorkStatus = true;
            CC_cConveyor.C_bRepairStatus = false;
        }


        private void initConveyor()
        {
            CC_cConveyor.C_pbConveer.P_iPosX = 250;
            CC_cConveyor.C_pbConveer.P_iPosY = 30;
        }


        private void initConveyor(int startY)
        {
            CC_cConveyor.C_pbConveer.P_iPosX = 250;
            CC_cConveyor.C_pbConveer.P_iPosY = startY;
        }

        // Операция конвеера
        public void conveyorOperation()
        {
            // Если на конвеере есть свободное место и детали в запасе есть,
            // то двигаем конвеер и добавляем новую деталь,
            // если на конвеере есть место, но есть деталь, которая дошла до конца, то её тоже удаляем
            // такое может случиться, если погрузчик будет занят другим конвеером и не успеет вовремя добавить детали
            // что приведёт к созданию пустых мест между деталями.
            if (CC_cConveyor.C_qConveyor.Count < (Models.Conveyors.C_iNumParts) 
                && CC_cConveyor.C_qReserve.Count > 0)
            {
                foreach (var pPart in CC_cConveyor.C_qConveyor)
                {
                    pPart.P_iPosX = pPart.P_iPosX + 3;
                    pPart.P_iPosY = 35 + CC_iStartY;
                }

                if (CC_cConveyor.C_qConveyor.Count == 0)
                {
                    CC_cConveyor.C_qConveyor.Enqueue(CC_cConveyor.C_qReserve.Pop());
                }
                else if ((CC_cConveyor.C_qConveyor.Peek().P_iPosX - 325) % Models.Conveyors.C_iStep == 0)
                {
                    CC_cConveyor.C_qConveyor.Enqueue(CC_cConveyor.C_qReserve.Pop());
                }
                if ((CC_cConveyor.C_qConveyor.Peek().P_iPosX - 325) >= (Models.Conveyors.C_iStep * 5 - 10))
                {
                    CC_cConveyor.C_qConveyor.Dequeue();
                }
            }
            // Если на конвеере нет свободного места,
            // то двигаем конвеер, удаляем готовую деталь и добавляем деталь новую деталь, если есть
            else if (CC_cConveyor.C_qConveyor.Count == Models.Conveyors.C_iNumParts)
            {
                foreach (var pPart in CC_cConveyor.C_qConveyor)
                {
                    pPart.P_iPosX = pPart.P_iPosX + 3;
                    pPart.P_iPosY = 35 + CC_iStartY;
                }

                if ((CC_cConveyor.C_qConveyor.Peek().P_iPosX - 325) >= (Models.Conveyors.C_iStep * 5 - 10))
                {
                    CC_cConveyor.C_qConveyor.Dequeue();
                    if (CC_cConveyor.C_qReserve.Count > 0)
                    {
                        CC_cConveyor.C_qConveyor.Enqueue(CC_cConveyor.C_qReserve.Pop());
                    }
                }
            }
            // Если на конвеере есть свободное место, но деталей в запасе нет, а на конвеере ещё есть,
            // то двигаем конвеер и удаляем готовую деталь
            else if (CC_cConveyor.C_qConveyor.Count < Models.Conveyors.C_iNumParts 
                && CC_cConveyor.C_qReserve.Count == 0 
                && CC_cConveyor.C_qConveyor.Count != 0)
            {
                foreach (var pPart in CC_cConveyor.C_qConveyor)
                {
                    pPart.P_iPosX = pPart.P_iPosX + 3;
                    pPart.P_iPosY = 35 + CC_iStartY;
                }

                if ((CC_cConveyor.C_qConveyor.Peek().P_iPosX - 325) >= (Models.Conveyors.C_iStep * 5 -10 ))
                {
                    CC_cConveyor.C_qConveyor.Dequeue();
                }
            }
        }


        public void conveyorIsBroken()
        {
            if (CC_rBreakdown.NextDouble() >= Models.Conveyors.C_fReliability)
            {
                CC_cConveyor.C_bWorkStatus = false;
            }
        }
    }
}
