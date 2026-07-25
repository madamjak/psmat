using PisaciStroj.Lexer;
using System.Collections.Generic;
using System.Text;

namespace Lexer.Algoritmy
{
    /// <summary>
    /// http://cgosorio.es/Seshat/ahoSethiUllman
    /// </summary>
    public class AhoSethiUllman
    {
        private ShuntingYard _shuntingYard;

        public AhoSethiUllman()
        {
            _shuntingYard = new ShuntingYard();
        }

        public DfaAutomaton BuildDfa(LexPravidlo pravidlo)
        {
            var astTree = _shuntingYard.BuildTree(pravidlo.Regex);

            astTree.PostorderTraversalToConstructFollowpos();

            var automaton = BuildAutomaton(astTree.Root.FirstPos, astTree.SymbolPos, astTree.FollowPos, astTree.CurrentLeafPos, astTree.Symbols);
            automaton.AkceptovanyToken = pravidlo.TypTokenu;

            return automaton;
        }

        public DfaAutomaton BuildDfaForSearch(string regex)
        {
            var astTree = _shuntingYard.BuildTree(regex);

            astTree.PostorderTraversalToConstructFollowpos();

            var automaton = BuildAutomaton(astTree.Root.FirstPos, astTree.SymbolPos, astTree.FollowPos, astTree.CurrentLeafPos, astTree.Symbols, false);

            return automaton;
        }

        private DfaAutomaton BuildAutomaton(
            HashSet<int> rootFirstPos,
            Dictionary<int, char> symbolPos,
            Dictionary<int,
            HashSet<int>> followPos,
            int lastPos,
            HashSet<char> symbols,
            bool deadStateToInitialState = false)
        {
            var states = new List<DfaState>();

            //keep track of new states sets of positions
            var stateSetOfPositions = new Dictionary<int, HashSet<int>>();

            //initial state, stateId is index in array
            var newStateIndex = 0;
            stateSetOfPositions.Add(newStateIndex, rootFirstPos);

            var initialState = new DfaState()
            {
                IsFinal = rootFirstPos.Contains(lastPos)
            };
            states.Add(initialState);

            //process initial state
            ConstructStateTransitions(symbolPos, followPos, stateSetOfPositions, newStateIndex, ref newStateIndex, initialState, states, lastPos, symbols, deadStateToInitialState);

            //to check till when to process states
            var numberOfStatesWithTransitions = 1;
            while (states.Count > numberOfStatesWithTransitions)
            {
                for (int i = numberOfStatesWithTransitions; i < states.Count; i++)
                {
                    ConstructStateTransitions(symbolPos, followPos, stateSetOfPositions, i, ref newStateIndex, states[i], states, lastPos, symbols, deadStateToInitialState);

                    numberOfStatesWithTransitions += i;
                }
            }

            return new DfaAutomaton
            {
                States = states.ToArray()
            };
        }

        private void ConstructStateTransitions(
            Dictionary<int, char> symbolPos,
            Dictionary<int, HashSet<int>> followPos,
            Dictionary<int, HashSet<int>> statesSetOfPositions,
            int currentStateIndex,
            ref int newStateIndex,
            DfaState state,
            List<DfaState> states,
            int lastPos,
            HashSet<char> symbols,
            bool deadStateToInitialState)
        {
            foreach (var symbol in symbols)
            {
                if (symbol == '\0')
                {
                    break;
                }

                var transitionStateIndex = -1;

                //get applicable followpos first
                var followPo = new HashSet<int>();
                foreach (var position in statesSetOfPositions[currentStateIndex])
                {
                    char symbolOnPosition;
                    if (symbolPos.TryGetValue(position, out symbolOnPosition))
                    {
                        if (symbolOnPosition == symbol)
                        {
                            followPo.UnionWith(followPos[position]);
                        }
                    }
                }

                if (followPo.Count == 0 && deadStateToInitialState == true)
                {
                    transitionStateIndex = 0;
                }
                else
                {
                    //check if such state already exists
                    foreach (var existingStates in statesSetOfPositions)
                    {
                        if (existingStates.Value.SetEquals(followPo))
                        {
                            transitionStateIndex = existingStates.Key;
                        }
                    }
                }

                //if it doesn't then create it
                if (transitionStateIndex == -1)
                {
                    newStateIndex++;

                    var newState = new DfaState()
                    {
                        IsFinal = followPo.Contains(lastPos),
                        IsDead = followPo.Count == 0
                    };
                    states.Add(newState);
                    statesSetOfPositions.Add(newStateIndex, followPo);

                    transitionStateIndex = newStateIndex;
                }

                state.Transitions.Add(symbol, transitionStateIndex);
            }
        }
    }
}
