using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;


namespace Conveyor.Models
{
    class Conveyors
    {
        public Queue<PictureBox> C_qConveyor = new Queue<PictureBox>();  // Детали конвеера
        public Stack<PictureBox> C_qReserve = new Stack<PictureBox>();  // Запас деталей конвеера
        public PictureBox C_pbConveer = new PictureBox();
        public const int C_iStep = 90;  // Шаг конвеера по оси Y
        public const int C_iNumParts = 5;   // Колличество деталей на конвеере
        public const int C_iHitbox = 100;   // Колличество единиц для починки
        public const float C_fReliability = 1.1f;   // Надёжность конвеера(вероятность не сломаться при движении деталей)
        public bool C_bLoad { get; set; }   // True - Конвеер загружен, False - пустой
        public bool C_bWorkStatus { get; set; }   // True - Конвеер исправен, False - сломан
    }
}
