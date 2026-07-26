using System;
using System.Collections.Generic;

namespace PisaciStroj.Pamat
{
    public interface IZasobnikOperacii
    {
        void Push(Operacia o);
        Operacia Pop();
        void Clear();
    }

    public class CSharpStack : IZasobnikOperacii
    {
        private Stack<Operacia> _stack;

        public CSharpStack()
        {
            _stack = new Stack<Operacia>();
        }

        public void Clear()
        {
            _stack.Clear();
        }

        public Operacia Pop()
        {
            return _stack.Pop();
        }

        public void Push(Operacia o)
        {
            _stack.Push(o);
        }
    }

    public class CustomCyclicStack : IZasobnikOperacii
    {
        private CyclicStack<Operacia> _stack;

        public CustomCyclicStack(int size)
        {
            _stack = new CyclicStack<Operacia>(size);
        }

        public void Clear()
        {
            _stack.Clear();
        }

        public Operacia Pop()
        {
            return _stack.Pop();
        }

        public void Push(Operacia o)
        {
            _stack.Push(o);
        }
    }

    internal class PamatOperacii
    {
        private IZasobnikOperacii _operacieNaVratenie;
        private IZasobnikOperacii _operacieNaZopakovanie;

        public int PocetOperaciiNaVratenie { get; private set; }
        public int PocetOperaciiNaZopakovanie { get; private set; }

        private int? _limit;

        private DateTime? _posledneUlozenie;
        private int _pocetOperaciiOdPoslUlozenia;

        public PamatOperacii(int? undoLimit = null)
        {
            if (undoLimit.HasValue)
            {
                _limit = undoLimit;
                _operacieNaVratenie = new CustomCyclicStack(undoLimit.Value);
                _operacieNaZopakovanie = new CustomCyclicStack(undoLimit.Value);
            }
            else
            {
                _operacieNaVratenie = new CSharpStack();
                _operacieNaZopakovanie = new CSharpStack();
            }
        }

        public void PridajOperaciuNaVratenie(Operacia operacia)
        {
            _pocetOperaciiOdPoslUlozenia++;

            _operacieNaVratenie.Push(operacia);

            if(!_limit.HasValue || PocetOperaciiNaVratenie < _limit.Value)
            {
                PocetOperaciiNaVratenie++;
            }
        }

        public void PridajOperaciuNaZopakovanie(Operacia operacia)
        {
            _pocetOperaciiOdPoslUlozenia--;

            _operacieNaZopakovanie.Push(operacia);

            PocetOperaciiNaZopakovanie++;
        }

        public Operacia OperaciaNaVratenie()
        {
            var operacia = _operacieNaVratenie.Pop();
            PocetOperaciiNaVratenie--;

            return operacia;
        }

        public Operacia OperaciaNaZopakovanie()
        {
            var operacia = _operacieNaZopakovanie.Pop();
            PocetOperaciiNaZopakovanie--;

            return operacia;
        }

        public void VycistiOperacieNaZopakovanie()
        {
            if (PocetOperaciiNaZopakovanie > 0)
            {
                _operacieNaZopakovanie.Clear();
                PocetOperaciiNaZopakovanie = 0;
            }
        }

        internal void SuborUlozeny()
        {
            _posledneUlozenie = DateTime.Now;
            _pocetOperaciiOdPoslUlozenia = 0;
        }

        internal bool MaZmenu()
        {
            var ulozeny = _pocetOperaciiOdPoslUlozenia == 0;

            return !ulozeny;
        }
    }
}
