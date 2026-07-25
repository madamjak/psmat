using System;
using System.Collections.Generic;
using System.Text;

namespace PisaciStroj.Pamat
{
    public class CyclicStack<T> where T : class
    {
        private int _size;
        private int _top = -1;
        private int _bottom;
        private T[] _array;

        public CyclicStack(int size)
        {
            _size = size;
            _array = new T[size];
        }

        public void Push(T item)
        {
            _top++;
            
            if (_top == _size)
            {
                _top = 0;
                _bottom = 1;
            }

            if (_bottom > 0)
            {
                _bottom++;
                if(_bottom == _size)
                {
                    _bottom = 0;
                }
            }

            _array[_top] = item;
        }

        public T Pop()
        {
            if (_top == -1)
            {
                throw new ApplicationException("Stack empty");
            }

            var item = _array[_top];

            _top--;
            if (_top < 0)
            {
                if(_bottom > 0)
                {
                    _top = _size - 1;
                }
                else
                {
                    _bottom = 0;
                }
            }
            //else
            //{
            //    if(_top < _bottom)
            //    {
            //        _top = -1;
            //        _bottom = 0;
            //    }
            //}

            return item;
        }

        public void Clear()
        {
            _array = new T[_size];
            _top = -1;
            _bottom = 0;
        }
    }
}
