using PisaciStroj.Lexer;
using System.Collections.Generic;

namespace Lexer.Algoritmy
{
    public class DfaAutomaton
    {
        public DfaState[] States { get; set; }

        public TypTokenu AkceptovanyToken { get; set; }
    }

    public class DfaState
    {
        public DfaState()
        {
            Transitions = new Dictionary<char, int>();
        }

        public bool IsFinal { get; set; }

        public bool IsDead { get; set; }

        public Dictionary<char, int> Transitions { get; set; }
    }

    public interface IDfaSimulator
    {
        bool ReadSymbol(char symbol);

        TypTokenu? IsAccepting();

        TypTokenu? IsPreviousStateAccepting();

        void Reset();
    }

    public class DfaSimulator : IDfaSimulator
    {
        private DfaAutomaton _dfa;

        private int _currentStateId;
        private int? _previousStateId;

        public DfaSimulator(DfaAutomaton dfa)
        {
            _dfa = dfa;
        }

        public bool ReadSymbol(char symbol)
        {
            if (_currentStateId >= 0)
            {
                int newState;
                if (!_dfa.States[_currentStateId].IsDead && _dfa.States[_currentStateId].Transitions.TryGetValue(symbol, out newState) && !_dfa.States[newState].IsDead)
                {
                    _previousStateId = _currentStateId;
                    _currentStateId = newState;

                    return true;
                }
                else
                {
                    _previousStateId = _currentStateId;
                    _currentStateId = -1;

                    return false;
                }
            }
            else
            {
                _previousStateId = _currentStateId;
                _currentStateId--;

                return false;
            }
        }

        public TypTokenu? IsAccepting()
        {
            return _currentStateId >= 0 && _dfa.States[_currentStateId].IsFinal ? _dfa.AkceptovanyToken : default(TypTokenu?);
        }

        public TypTokenu? IsPreviousStateAccepting()
        {
            return _previousStateId.HasValue && _previousStateId >= 0 && _dfa.States[_previousStateId.Value].IsFinal ? _dfa.AkceptovanyToken : default(TypTokenu?);
        }

        public void Reset()
        {
            _currentStateId = 0;
            _previousStateId = null;
        }
    }

    public class MultipleDfaSimulator : IDfaSimulator
    {
        private readonly List<IDfaSimulator> _dfaSimulators;

        public MultipleDfaSimulator(List<IDfaSimulator> dfaSimulators)
        {
            _dfaSimulators = dfaSimulators;
        }

        public TypTokenu? IsAccepting()
        {
            TypTokenu? result = null;
            foreach (var simulator in _dfaSimulators)
            {
                result = simulator.IsAccepting();
                if (result.HasValue)
                {
                    break;
                }
            }

            return result;
        }

        public TypTokenu? IsPreviousStateAccepting()
        {
            TypTokenu? result = null;
            foreach (var simulator in _dfaSimulators)
            {
                result = simulator.IsPreviousStateAccepting();
                if (result.HasValue)
                {
                    break;
                }
            }

            return result;
        }

        public bool ReadSymbol(char symbol)
        {
            var symbolRead = false;
            foreach (var simulator in _dfaSimulators)
            {
                var canRead = simulator.ReadSymbol(symbol);
                if (canRead && !symbolRead)
                {
                    symbolRead = true;
                }
            }

            return symbolRead;
        }

        public void Reset()
        {
            foreach (var sim in _dfaSimulators)
            {
                sim.Reset();
            }
        }
    }
}
