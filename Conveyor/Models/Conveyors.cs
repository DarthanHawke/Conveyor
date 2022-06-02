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
        public Queue<Parts> C_qConveyor = new Queue<Parts>();  // Детали конвеера
        public Stack<Parts> C_qReserve = new Stack<Parts>();  // Запас деталей конвеера
        public Parts C_pbConveer = new Parts();
        public const int C_iStep = 90;  // Шаг конвеера по оси Y
        public const int C_iNumParts = 5;   // Колличество деталей на конвеере
        public const int C_iHitbox = 100;   // Колличество единиц для починки
        public const float C_fReliability = 0.991f;   // Надёжность конвеера(вероятность не сломаться при движении деталей)
        public bool C_bLoad { get; set; }   // True - Конвеер загружен, False - пустой
        public bool C_bWorkStatus { get; set; }   // True - Конвеер исправен, False - сломан
    }
}
