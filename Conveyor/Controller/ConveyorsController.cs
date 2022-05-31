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
        public Models.Conveyors CC_cConveyor = new Models.Conveyors();
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
            initConveyor();
        }

        private void initializeConveyorsController()
        {
            CC_cConveyor.C_bLoad = false;
            CC_cConveyor.C_bWorkStatus = true;
        }

        private void initializeConveyorsController(int startY)
        {
            CC_iStartY = startY;
            CC_cConveyor.C_bLoad = false;
            CC_cConveyor.C_bWorkStatus = true;
        }


        private void initConveyor()
        {
            CC_cConveyor.C_pbConveer.Name = "conveer";
            CC_cConveyor.C_pbConveer.Image = Image.FromFile(@"../../Resources/conveyor.png");
            CC_cConveyor.C_pbConveer.Size = new System.Drawing.Size(700, 150);
            CC_cConveyor.C_pbConveer.Location = new Point(250, CC_iStartY);
            CC_cConveyor.C_pbConveer.SizeMode = PictureBoxSizeMode.Zoom;
        }

        // Операция конвеера
        public void conveyorOperation()
        {
            // Если на конвеере есть свободное место и детали в запасе есть,
            // то двигаем конвеер и добавляем новую деталь
            if (CC_cConveyor.C_qConveyor.Count < Models.Conveyors.C_iNumParts 
                && CC_cConveyor.C_qReserve.Count > 0)
            {
                foreach (var pPart in CC_cConveyor.C_qConveyor)
                {
                    CC_cConveyor.C_pbConveer.Invoke((MethodInvoker)delegate
                    {
                        pPart.Location = new Point(pPart.Location.X + 3, 52 + CC_iStartY);
                    });
                }
                if (CC_cConveyor.C_qConveyor.Count == 0)
                {
                    CC_cConveyor.C_qConveyor.Enqueue(CC_cConveyor.C_qReserve.Pop());
                }
                else if ((CC_cConveyor.C_qConveyor.Peek().Location.X - 325) % Models.Conveyors.C_iStep == 0)
                {
                    CC_cConveyor.C_qConveyor.Enqueue(CC_cConveyor.C_qReserve.Pop());
                }
            }
            // Если на конвеере нет свободного места, а детали в запасе есть,
            // то двигаем конвеер, удаляем готовую деталь и добавляем деталь новую деталь
            if (CC_cConveyor.C_qConveyor.Count == Models.Conveyors.C_iNumParts)
            {
                foreach (var pPart in CC_cConveyor.C_qConveyor)
                {
                    CC_cConveyor.C_pbConveer.Invoke((MethodInvoker)delegate
                    {
                        pPart.Location = new Point(pPart.Location.X + 3, 52 + CC_iStartY);
                    });
                }
                if ((CC_cConveyor.C_qConveyor.Peek().Location.X - 325) % Models.Conveyors.C_iStep == 0)
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
            if (CC_cConveyor.C_qConveyor.Count < Models.Conveyors.C_iNumParts 
                && CC_cConveyor.C_qReserve.Count == 0 
                && CC_cConveyor.C_qConveyor.Count != 0)
            {
                foreach (var pPart in CC_cConveyor.C_qConveyor)
                {
                    CC_cConveyor.C_pbConveer.Invoke((MethodInvoker)delegate
                    {
                        pPart.Location = new Point(pPart.Location.X + 3, 52 + CC_iStartY);
                    });
                }
                if ((CC_cConveyor.C_qConveyor.Peek().Location.X - 325) >= Models.Conveyors.C_iStep * 5)
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
