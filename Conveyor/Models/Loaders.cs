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
        public PictureBox L_pbLoader = new PictureBox();
        public const int L_iBatch = 12;  // Количество товаров загружаемых погрузчиком за раз
        public int L_iLoadBatch { get; set; } // Счётчик загруженных товаров
        public bool L_bLoading { get; set; }    // True - погрузка производится, False - погурзка не производится
    }
}
