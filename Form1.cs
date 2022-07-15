using System;
using System.IO;
using System.Windows.Forms;

namespace DES
{
    public partial class FormMain : Form
    {
        public FormMain()
        {
            InitializeComponent();
        }

        private struct binaryLR
        {
            public string L;
            public string R;
        }

        // размер символа бит
        private const byte charLength = 16;
        // размер блока DES
        private const byte blockLength = 64;
        // длина ключа
        private const byte keyLength = blockLength / charLength;
        // количество раундов
        private const byte rounds = 16;

        public string encryptDES(string text, string key)
        {
            // дополнение строки до длины кратной блоку
            text = stringExtend(text);
            // перевод в биты
            text = stringToBinary(text);
            // перевод в блоки битов
            string[] blocks;
            blocks = binaryToBlocks(text);
            // начальная перестановка
            blocks = doPermutation(blocks, initialPermutation);
            // перевод в блоки с левой-правой частью
            binaryLR[] blocksLR;
            blocksLR = cutBlocks(blocks);
            // вычисление ключей раунда
            string[] roundKeys;
            roundKeys = createRoundKeys(key);
            // проведение раундов
            for (int i = 0; i < rounds; i++)
            {
                blocksLR = encryptRound(blocksLR, roundKeys[i]);
            }
            // соединение левых-правых частей
            for (int i = 0; i < blocks.Length; i++)
            {
                blocks[i] = blocksLR[i].L + blocksLR[i].R;
            }
            // конечная перестановка
            blocks = doPermutation(blocks, inversePermutation);
            // соединение блоков
            text = blocksToBinary(blocks);
            // биты в текст
            text = binaryToString(text);
            return text;
        }

        public string decryptDES(string text, string key)
        {
            // перевод в биты
            text = stringToBinary(text);
            // перевод в блоки битов
            string[] blocks;
            blocks = binaryToBlocks(text);
            // конечная перестановка
            blocks = doPermutation(blocks, initialPermutation);
            // перевод в блоки с левой-правой частью
            binaryLR[] blocksLR;
            blocksLR = cutBlocks(blocks);
            // вычисление ключей раунда
            string[] roundKeys;
            roundKeys = createRoundKeys(key);
            // проведение раундов
            for (int i = 0; i < rounds; i++)
            {
                blocksLR = decryptRound(blocksLR, roundKeys[rounds - i - 1]);
            }
            // соединение левых-правых частей
            for (int i = 0; i < blocks.Length; i++)
            {
                blocks[i] = blocksLR[i].L + blocksLR[i].R;
            }
            // начальная перестановка
            blocks = doPermutation(blocks, inversePermutation);
            // соединение блоков
            text = blocksToBinary(blocks);
            // биты в текст
            text = binaryToString(text);
            return text;
        }

        private string stringExtend(string text)
        {
            if (text.Length % (blockLength / charLength) != 0)
            {
                // символ конца строки
                while (text.Length % (blockLength / charLength) != 0)
                {
                    text += '\0';
                }
            }
            return text;
        }

        // начальная перестановка
        private readonly byte[] initialPermutation =
           {58, 50, 42, 34, 26, 18, 10, 2,
            60, 52, 44, 36, 28, 20, 12, 4,
            62, 54, 46, 38, 30, 22, 14, 6,
            64, 56, 48, 40, 32, 24, 16, 8,
            57, 49, 41, 33, 25, 17, 9, 1,
            59, 51, 43, 35, 27, 19, 11, 3,
            61, 53, 45, 37, 29, 21, 13, 5,
            63, 55, 47, 39, 31, 23, 15, 7};

        // обратная перестановка
        private readonly byte[] inversePermutation =
           {40, 8, 48, 16, 56, 24, 64, 32,
            39, 7, 47, 15, 55, 23, 63, 31,
            38, 6, 46, 14, 54, 22, 62, 30,
            37, 5, 45, 13, 53, 21, 61, 29,
            36, 4, 44, 12, 52, 20, 60, 28,
            35, 3, 43, 11, 51, 19, 59, 27,
            34, 2, 42, 10, 50, 18, 58, 26,
            33, 1, 41, 9, 49, 17, 57, 25};

        private string doPermutation(string binary, byte[] permutation)
        {
            // место для результата
            string result = "";
            // перестановка
            for (int i = 0; i < permutation.Length; i++)
            {
                result += binary[permutation[i] - 1];
            }
            return result;
        }

        private string[] doPermutation(string[] blocks, byte[] permutation)
        {
            // место для результата
            string[] result = new string[blocks.Length];
            // зануление
            for (int i = 0; i < result.Length; i++)
            {
                result[i] = "";
            }
            // обход всех блоков
            for (int block = 0; block < blocks.Length; block++)
            {
                // перестановка
                for (int i = 0; i < permutation.Length; i++)
                {
                    result[block] += blocks[block][permutation[i] - 1];
                }
            }
            return result;
        }

        private string stringToBinary(string text)
        {
            // место для результата
            string result = "";
            // текущий символ
            string current;
            for (int i = 0; i < text.Length; i++)
            {
                // перевод в двоичный вид
                current = Convert.ToString(text[i], 2);
                // дополнение до нужной длины
                while (current.Length < charLength)
                {
                    current = '0' + current;
                }
                // запись в результат
                result += current;
            }
            return result;
        }

        private string binaryToString(string binary)
        {
            // место для результата
            string result = "";
            // текущий символ
            int current;
            // проход по байтам
            for (int b = 0; b <= binary.Length - charLength; b = b + charLength)
            {
                // текущий символ
                current = 0;
                for (int i = 0; i < charLength; i++)
                {
                    // побитовый перевод числа в 10-ю систему счисления
                    current += Convert.ToInt32(binary[b + i].ToString()) * Convert.ToInt32(Math.Pow(2, charLength - i - 1));
                }
                // запись в результат
                result += (char)current;
            }
            return result;
        }

        private string[] binaryToBlocks(string binary)
        {
            // создание блоков
            string[] blocks = new string[binary.Length / blockLength];
            // заполнение блоков
            for (int i = 0; i < blocks.Length; i++)
            {
                blocks[i] = binary.Substring(i * blockLength, blockLength);
            }
            return blocks;
        }

        private string blocksToBinary(string[] blocks)
        {
            // место для результата
            string result = "";
            // объединение в строку
            for (int i = 0; i < blocks.Length; i++)
            {
                result += blocks[i];
            }
            return result;
        }
        private string[] cutBinary(string binary, int count)
        {
            // место для результата
            string[] result = new string[count];
            // зануление
            for (int i = 0; i < result.Length; i++)
            {
                result[i] = "";
            }
            // длина блока
            int blockLength_ = binary.Length / count;
            // обход блоков
            for (int block = 0; block < binary.Length - blockLength_; block = block + blockLength_)
            {
                // запись в блок
                for (int i = 0; i < count; i++)
                {
                    result[i] += binary[block + i];
                }
            }
            return result;
        }

        private binaryLR cutBinary(string binary)
        {
            // место для результата
            binaryLR result;
            // левая половина
            result.L = binary.Substring(0, binary.Length / 2);
            // правая половина
            result.R = binary.Substring(binary.Length / 2, binary.Length / 2);
            return result;
        }

        private binaryLR[] cutBlocks(string[] blocks)
        {
            // место для результата
            binaryLR[] result = new binaryLR[blocks.Length];
            // обход блоков
            for (int i = 0; i < blocks.Length; i++)
            {
                // левая половина
                result[i].L = blocks[i].Substring(0, blocks[i].Length / 2);
                // правая половина
                result[i].R = blocks[i].Substring(blocks[i].Length / 2, blocks[i].Length / 2);
            }
            return result;
        }

        private readonly byte[] permutedChoice1 =
           {57, 49, 41, 33, 25, 17, 9,
            1, 58, 50, 42, 34, 26, 18,
            10, 2, 59, 51, 43, 35, 27,
            19, 11, 3, 60, 52, 44, 36,
            63, 55, 47, 39, 31, 23, 15,
            7, 62, 54, 46, 38, 30, 22,
            14, 6, 61, 53, 45, 37, 29,
            21, 13, 5, 28, 20, 12, 4};

        private readonly byte[] permutedChoice2 =
           {14, 17, 11, 24, 1, 5,
            3, 28, 15, 6, 21, 10,
            23, 19, 12, 4, 26, 8,
            16, 7, 27, 20, 13, 2,
            41, 52, 31, 37, 47, 55,
            30, 40, 51, 45, 33, 48,
            44, 49, 39, 56, 34, 53,
            46, 42, 50, 36, 29, 32,};

        private readonly byte[] leftShifts = { 1, 1, 2, 2, 2, 2, 2, 2, 1, 2, 2, 2, 2, 2, 2, 1 };

        private string[] createRoundKeys(string key)
        {
            // ключ в биты
            key = stringToBinary(key);
            // перестановка ключа
            key = doPermutation(key, permutedChoice1);
            // левая-правая части ключа
            binaryLR keyLR;
            keyLR = cutBinary(key);
            // вычисление ключей
            string[] roundKeys = new string[rounds];
            for (int round = 0; round < rounds; round++)
            {
                // сдвиги
                for (int i = 0; i < leftShifts[round]; i++)
                {
                    keyLR = leftShift(keyLR);
                }
                // ключ раунда
                roundKeys[round] = doPermutation(keyLR.L + keyLR.R, permutedChoice2);
            }
            return roundKeys;
        }

        private binaryLR leftShift(binaryLR keyLR)
        {
            // циклический сдвиг
            keyLR.L = keyLR.L + keyLR.L[0];
            keyLR.L = keyLR.L.Remove(0, 1);
            keyLR.R = keyLR.R + keyLR.R[0];
            keyLR.R = keyLR.R.Remove(0, 1);
            return keyLR;
        }

        // расширение ключа
        private readonly byte[] E =
           {32, 1, 2, 3, 4, 5,
            4, 5, 6, 7, 8, 9,
            8, 9, 10, 11, 12, 13,
            12, 13, 14, 15, 16, 17,
            16, 17, 18, 19, 20, 21,
            20, 21, 22, 23, 24, 25,
            24, 25, 26, 27, 28, 29,
            28, 29, 30, 31, 32, 1};

        private readonly byte[] P =
           {16, 7, 20, 21,
            29, 12, 28, 17,
            1, 15, 23, 26,
            5, 18, 31, 10,
            2, 8, 24, 14,
            32, 27, 3, 9,
            19, 13, 30, 6,
            22, 11, 4, 25};

        private string cryptFunction(string text, string key)
        {
            // E перестановка
            text = doPermutation(text, E);
            // XOR
            text = stringXOR(text, key);
            // S перестановки
            string[] buff = new string[8];
            // разбиение на 8-м массивов
            buff = cutBinary(text, 8);
            buff[0] = doSPermutation(buff[0], S1);
            buff[1] = doSPermutation(buff[1], S2);
            buff[2] = doSPermutation(buff[2], S3);
            buff[3] = doSPermutation(buff[3], S4);
            buff[4] = doSPermutation(buff[4], S5);
            buff[5] = doSPermutation(buff[5], S6);
            buff[6] = doSPermutation(buff[6], S7);
            buff[7] = doSPermutation(buff[7], S8);
            // дополнение до 4-х символов
            for (int i = 0; i < buff.Length; i++)
            {
                while (buff[i].Length < 4)
                {
                    buff[i] = '0' + buff[i];
                }
            }
            // конкатенация
            text = blocksToBinary(buff);
            // P перестановка
            text = doPermutation(text, P);
            return text;
        }

        private string stringXOR(string arg1, string arg2)
        {
            // место для результата
            string result = "";
            // обход строк
            for (int i = 0; i < arg1.Length; i++)
            {
                // если оба нули то ноль
                if (arg1[i] == '0' && arg2[i] == '0')
                {
                    result += '0';
                }
                // если оба единицы то ноль
                else if (arg1[i] == '1' && arg2[i] == '1')
                {
                    result += '0';
                }
                // единица
                else
                {
                    result += '1';
                }
            }
            return result;
        }

        private readonly byte[,] S1 =
           {{14, 4, 13, 1, 2, 15, 11, 8, 3, 10, 6, 12, 5, 9, 0, 7},
            {0, 15, 7, 4, 14, 2, 13, 1, 10, 6, 12, 11, 9, 5, 3, 8},
            {4, 1, 14, 8, 13, 6, 2, 11, 15, 12, 9, 7, 3, 10, 5, 0},
            {15, 12, 8, 2, 4, 9, 1, 7, 5, 11, 3, 14, 10, 0, 6, 13}};

        private readonly byte[,] S2 =
           {{15, 1, 8, 14, 6, 11, 3, 4, 9, 7, 2, 13, 12, 0, 5, 10},
            {3, 13, 4, 7, 15, 2, 8, 14, 12, 0, 1, 10, 6, 9, 11, 5},
            {0, 14, 7, 11, 10, 4, 13, 1, 5, 8, 12, 6, 9, 3, 2, 15},
            {13, 8, 10, 1, 3, 15, 4, 2, 11, 6, 7, 12, 0, 5, 14, 9}};

        private readonly byte[,] S3 =
           {{10, 0, 9, 14, 6, 3, 15, 5, 1, 13, 12, 7, 11, 4, 2, 8},
            {13, 7, 0, 9, 3, 4, 6, 10, 2, 8, 5, 14, 12, 11, 15, 1},
            {13, 6, 4, 9, 8, 15, 3, 0, 11, 1, 2, 12, 5, 10, 14, 7},
            {1, 10, 13, 0, 6, 9, 8, 7, 4, 15, 14, 3, 11, 5, 2, 12}};

        private readonly byte[,] S4 =
           {{7, 13, 14, 3, 0, 6, 9, 10, 1, 2, 8, 5, 11, 12, 4, 15},
            {13, 8, 11, 5, 6, 15, 0, 3, 4, 7, 2, 12, 1, 10, 14, 9},
            {10, 6, 9, 0, 12, 11, 7, 13, 15, 1, 3, 14, 5, 2, 8, 4},
            {3, 15, 0, 6, 10, 1, 13, 8, 9, 4, 5, 11, 12, 7, 2, 14}};

        private readonly byte[,] S5 =
           {{2, 12, 4, 1, 7, 10, 11, 6, 8, 5, 3, 15, 13, 0, 14, 9},
            {14, 11, 2, 12, 4, 7, 13, 1, 5, 0, 15, 10, 3, 9, 8, 6},
            {4, 2, 1, 11, 10, 13, 7, 8, 15, 9, 12, 5, 6, 3, 0, 14},
            {11, 8, 12, 7, 1, 14, 2, 13, 6, 15, 0, 9, 10, 4, 5, 3}};

        private readonly byte[,] S6 =
           {{12, 1, 10, 15, 9, 2, 6, 8, 0, 13, 3, 4, 14, 7, 5, 11},
            {10, 15, 4, 2, 7, 12, 9, 5, 6, 1, 13, 14, 0, 11, 3, 8},
            {9, 14, 15, 5, 2, 8, 12, 3, 7, 0, 4, 10, 1, 13, 11, 6},
            {4, 3, 2, 12, 9, 5, 15, 10, 11, 14, 1, 7, 6, 0, 8, 13}};

        private readonly byte[,] S7 =
           {{4, 11, 2, 14, 15, 0, 8, 13, 3, 12, 9, 7, 5, 10, 6, 1},
            {13, 0, 11, 7, 4, 9, 1, 10, 14, 3, 5, 12, 2, 15, 8, 6},
            {1, 4, 11, 13, 12, 3, 7, 14, 10, 15, 6, 8, 0, 5, 9, 2},
            {6, 11, 13, 8, 1, 4, 10, 7, 9, 5, 0, 15, 14, 2, 3, 12}};

        private readonly byte[,] S8 =
           {{13, 2, 8, 4, 6, 15, 11, 1, 10, 9, 3, 14, 5, 0, 12, 7},
            {1, 15, 13, 8, 10, 3, 7, 4, 12, 5, 6, 11, 0, 14, 9, 2},
            {7, 11, 4, 1, 9, 12, 14, 2, 0, 6, 10, 13, 15, 3, 5, 8},
            {2, 1, 14, 7, 4, 10, 8, 13, 15, 12, 9, 0, 3, 5, 6, 11}};

        private string doSPermutation(string binary, byte[,] permutation)
        {
            // первый и последний биты
            string firstAndLastBits = binary[0].ToString() + binary[5].ToString();
            // средние 4-е бита
            string middleBits = binary[1].ToString() + binary[2].ToString() + binary[3].ToString() + binary[4].ToString();

            // строка
            int row = 0;
            for (int i = 0; i < firstAndLastBits.Length; i++)
            {
                // побитовый перевод числа в 10-ю систему счисления
                row += Convert.ToInt32(binary[i].ToString()) * Convert.ToInt32(Math.Pow(2, firstAndLastBits.Length - i - 1));
            }

            // столбец
            int col = 0;
            for (int i = 0; i < middleBits.Length; i++)
            {
                // побитовый перевод числа в 10-ю систему счисления
                col += Convert.ToInt32(binary[i].ToString()) * Convert.ToInt32(Math.Pow(2, middleBits.Length - i - 1));
            }

            // перевод в двоичную систему счисления
            return Convert.ToString(permutation[row, col], 2);
        }

        private binaryLR[] encryptRound(binaryLR[] binary, string roundKey)
        {
            // место для результата
            binaryLR[] result = new binaryLR[binary.Length];
            // раунд
            for (int i = 0; i < binary.Length; i++)
            {
                result[i].L = binary[i].R;
                result[i].R = stringXOR(binary[i].L, cryptFunction(binary[i].R, roundKey));
            }
            return result;
        }

        private binaryLR[] decryptRound(binaryLR[] binary, string roundKey)
        {
            // место для результата
            binaryLR[] result = new binaryLR[binary.Length];
            // раунд
            for (int i = 0; i < binary.Length; i++)
            {
                result[i].R = binary[i].L;
                result[i].L = stringXOR(binary[i].R, cryptFunction(binary[i].L, roundKey));
            }
            return result;
        }

        private const string KEY_SHORT_ERROR = "Ключ слишком короткий, будет дополнен.";
        private const string KEY_LONG_ERROR = "Ключ слишком длинный, будет укорочен.";

        private string testKey(string key)
        {
            if (key.Length < keyLength)
            {
                MessageBox.Show(KEY_SHORT_ERROR);
                // добавление мусора
                Random rand = new Random();
                while (key.Length != keyLength)
                {
                    key += (char)rand.Next(65, 90);
                }
            }
            else if (key.Length > keyLength)
            {
                MessageBox.Show(KEY_LONG_ERROR);
                // укорачивание
                key = key.Substring(0, keyLength);
            }
            return key;
        }

        // кнопки
        private void buttonEncrypt_Click(object sender, EventArgs e)
        {
            // проверка ключа
            string key;
            key = this.key.Text;
            key = testKey(key);
            this.key.Text = key;
            
            // шифрование
            string text; 
            text = this.textInputOutput.Text;
            text = encryptDES(text, key);
            this.textInputOutput.Text = text;
        }

        private void buttonDecrypt_Click(object sender, EventArgs e)
        {
            // проверка ключа
            string key;
            key = this.key.Text;
            key = testKey(key);
            this.key.Text = key;

            // расшифрование
            string text;
            text = this.textInputOutput.Text;
            text = decryptDES(text, key);
            this.textInputOutput.Text = text;
        }

        private void buttonSave_Click(object sender, EventArgs e)
        {
            SaveFileDialog saveFileDialog = new SaveFileDialog();
            saveFileDialog.FileName = "file.txt";

            if (saveFileDialog.ShowDialog() == DialogResult.OK)
            {
                File.WriteAllText(saveFileDialog.FileName, textInputOutput.Text);
            }
        }

        private void buttonLoad_Click(object sender, EventArgs e)
        {
            OpenFileDialog openFileDialog = new OpenFileDialog();
            openFileDialog.FileName = "file.txt";
            openFileDialog.Filter = "Text|*.txt";

            if (openFileDialog.ShowDialog() == DialogResult.OK)
            {
                textInputOutput.Text = File.ReadAllText(openFileDialog.FileName);
            }
        }
    }
}
