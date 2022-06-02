using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;


namespace Conveyor.Models
{
    class Loaders
    {
        public Parts L_pbLoader = new Parts();
        public Parts L_pbParts = new Parts();
        public const int L_iBatch = 9;  // Количество товаров загружаемых погрузчиком за раз
        public int L_iLoadBatch { get; set; } // Счётчик загруженных товаров
        public bool L_bLoading { get; set; }    // True - погрузка производится, False - погурзка не производится
    }
}
