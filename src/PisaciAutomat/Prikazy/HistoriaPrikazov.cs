using System.Collections.Generic;

namespace PisaciAutomat.Prikazy
{
    internal class HistoriaPrikazov
    {
        private Stack<string> _operacieNaVratenie;
        private Stack<string> _operacieNaZopakovanie;

        private List<string> _historia;

        public HistoriaPrikazov()
        {
            _operacieNaVratenie = new Stack<string>();
            _operacieNaZopakovanie = new Stack<string>();
            _historia = new List<string>();
            _operaciaHistorieTop = -1;
    }
        public void PridajOperaciuNaVratenie(string operacia)
        {
            _operacieNaVratenie.Push(operacia);
        }

        public void PridajOperaciuNaZopakovanie(string operacia)
        {
            _operacieNaZopakovanie.Push(operacia);
        }

        private int _operaciaHistorieTop = -1;
        public void PridajOperaciuDoHistorie(string operacia)
        {
            if(_operaciaHistorieTop < 0 || _historia[_operaciaHistorieTop] != operacia)
            {
                _historia.Add(operacia);
            }

            _operaciaHistorieTop = _historia.Count;
        }

        public string PoslednaOperaciaHistorie()
        {
            if (_operaciaHistorieTop < 0)
            {
                return null;
            }

            var o = _historia[_operaciaHistorieTop];
            _operaciaHistorieTop--;

            return o;
        }

        public int PocetOperaciiNaVratenie => _operacieNaVratenie.Count;
        public string OperaciaNaVratenie()
        {
            var operacia = _operacieNaVratenie.Pop();

            return operacia;
        }

        public int PocetOperaciiNaZopakovanie => _operacieNaZopakovanie.Count;
        public string OperaciaNaZopakovanie()
        {
            var operacia = _operacieNaZopakovanie.Pop();

            return operacia;
        }

        public void VycistiOperacieNaZopakovanie()
        {
            if (_operacieNaZopakovanie.Count > 0)
            {
                _operacieNaZopakovanie.Clear();
            }
            if (_operacieNaVratenie.Count > 0)
            {
                _operacieNaVratenie.Clear();
            }

            _operaciaHistorieTop = _historia.Count - 1;
        }
    }
}
