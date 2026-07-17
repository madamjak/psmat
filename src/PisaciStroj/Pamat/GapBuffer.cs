using System;

namespace PisaciStroj.Pamat
{
    public class GapBuffer
    {
        private char[] _array;

        private int _length;
        private int _gapSize;
        private int _gapPosition;

        public GapBuffer()
        {
            _array = new char[80];
            _gapSize = 80;
        }

        public void Append(char z)
        {
            Insert(z, _length);
        }

        public void Append(string s)
        {
            foreach (var c in s)
            {
                Append(c);
            }
        }

        public char CharAt(int index)
        {
            if (index == _length)
            {
                return '\0';
            }

            if (_length == 0)
            {
                return '\0';
            }

            if (index >= _gapPosition)
            {
                index += _gapSize;
            }

            return _array[index];
        }

        public void Delete(int index)
        {
            if (_gapPosition != index)
            {
                MoveGapPosition(index);
            }

            _length--;
            _gapSize++;

            //var ratio = Math.Round((decimal)(_array.Length / _gapSize), 1);
            //if (ratio < 1.5M)
            //{
            //    ZmensiPole();
            //}
        }

        public void Delete(int start, int length)
        {
            var c = 0;
            while (true)
            {
                if (c == length)
                {
                    break;
                }

                Delete(start);
                c++;
            }
        }

        public void Insert(char z, int index)
        {
            if (_gapSize == 0)
            {
                ZvacsiPole();
            }

            if (_gapPosition != index)
            {
                MoveGapPosition(index);
            }

            _array[index] = z;
            _length++;
            _gapSize--;
            _gapPosition++;
        }

        private void MoveGapPosition(int index)
        {
            if (index == _gapPosition)
            {
                return;
            }

            if (index < _gapPosition)
            {
                while (true)
                {
                    if (_gapPosition == index)
                    {
                        break;
                    }

                    var charBeforeGap = _array[_gapPosition - 1];
                    _gapPosition--;
                    _array[_gapPosition + _gapSize] = charBeforeGap;
                }
            }
            else
            {
                while (true)
                {
                    if (_gapPosition == index)
                    {
                        break;
                    }

                    var charAfterGap = _array[_gapPosition + _gapSize];
                    _array[_gapPosition] = charAfterGap;
                    _gapPosition++;
                }
            }
        }

        private void ZvacsiPole()
        {
            var dlzka = _array.Length;
            var novaDlzka = (int)Math.Ceiling(dlzka * 1.5M);

            _gapSize = novaDlzka - dlzka;

            var novePole = new char[novaDlzka];
            for (int i = 0; i < _array.Length; i++)
            {
                if (i >= _gapPosition)
                {
                    novePole[i + _gapSize] = _array[i];
                }
                else
                {
                    novePole[i] = _array[i];
                }
            }

            _array = novePole;
        }

        private void ZmensiPole()
        {
            var dlzka = _array.Length;
            var novaDlzka = (int)Math.Ceiling(dlzka / 1.5M);

            var novaGapSize = novaDlzka - Length();

            var novePole = new char[novaDlzka];
            var i = 0;
            var j = 0;
            while (true)
            {
                if (j == _length)
                {
                    break;
                }
                if (i == _gapPosition)
                {
                    i += _gapSize;
                    j += novaGapSize;
                }

                novePole[j] = _array[i];
                i++;
                j++;
            }

            _array = novePole;
            _gapSize = novaGapSize;
        }

        public void Insert(string s, int index)
        {
            var i = index;
            foreach (var c in s)
            {
                Insert(c, i);
                i++;
            }
        }

        public string Read()
        {
            var a = new char[_length];
            for (int i = 0; i < _length; i++)
            {
                a[i] = CharAt(i);
            }

            return new string(a);
        }

        public string Read(int index)
        {
            return Read(index, _length - index);
        }

        public string Read(int index, int length)
        {
            if(index + length > _length)
            {
                length = _length - index;
            }

            var a = new char[length];
            var c = index;
            for (int i = 0; i < length; i++)
            {
                a[i] = CharAt(c);
                c++;
            }

            return new string(a);
        }

        public int Length()
        {
            return _length;
        }
    }
}
