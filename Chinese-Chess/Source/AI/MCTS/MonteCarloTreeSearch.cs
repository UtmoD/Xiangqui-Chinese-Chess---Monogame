﻿using ChineseChess.Source.AI.Minimax;
using ChineseChess.Source.GameObjects.Chess;
using ChineseChess.Source.GameRule;
using ChineseChess.Source.Players;
using Microsoft.Xna.Framework;
using System;
using System.Collections.Generic;
using System.ComponentModel.Design;
using System.Linq;

namespace ChineseChess.Source.AI.MCTS
{
    public class MonteCarloTreeSearch : IMoveStrategy
    {
        public int PositionsEvaluated { get; protected set; }

        public string Name { get; protected set; }

        public Team Player { get; protected set; }

        private readonly int _simulations = 260;

        public MonteCarloTreeSearch(Team player)
        {
            Player = player;
            Name = "MonteCarloTreeSearch";
        }

        public (Point, Point) Search(BoardState state, int depth, GameTime gameTime)
        {
            PositionsEvaluated = 0;
            return UCTSearch(state, depth);
        }


        private (Point, Point) UCTSearch(BoardState state, int depth)
        {
            var currentPlayer = Player == Team.RED;
            var rootNode = new Node(null, state, (Point.Zero, Point.Zero), currentPlayer);
            for (int i = 0; i < _simulations; ++i)
            {
                var v = TreePolicy(rootNode);
                var vState = v.State.Clone();
                var reward = DefaultPolicy(vState, v.CurrentPlayer, depth);
                BackUp(v, reward);
            }
            //return rootNode.Children.OrderByDescending(n => n.Visits)
            //                      .ThenByDescending(n => n.TotalScore)
            //                      .ToList()[0].FromTo;
            return BestChild(rootNode).FromTo;
            //return rootNode.Children.OrderByDescending(n => n.TotalScore)
            //                      .ThenByDescending(n => n.Visits).ToList()[0].FromTo;
        }

        private static Node TreePolicy(Node v)
        {
            while (!IsTerminal(v.State))
            {
                if (!IsFullyExpanded(v)) return Expand(v);
                else v = BestChild(v);
            }
            return v;
        }

        private static bool IsFullyExpanded(Node v)
        {
            if (v.Children == null) return false;
            return v.Children.Where(n => n.Visits == 0).ToList().Count == 0;
        }

        private static Node Expand(Node v)
        {
            if (v.Children == null)
            {
                v.Children = new List<Node>();
                var untriedActions = (from piece in v.State.GetPieces(v.CurrentPlayer)
                                      from move in v.State.GetLegalMoves(piece)
                                      select (piece, move)).ToList();
                foreach (var action in untriedActions)
                {
                    var s = v.State.SimulateMove(action.piece, action.move);
                    var v_ = new Node(v, s, action, !v.CurrentPlayer);
                    v.State.Undo();
                    v.Children.Add(v_);
                }
            }

            var unvisitNodes = v.Children.Where(n => n.Visits == 0).ToList();
            return unvisitNodes[new Random().Next(unvisitNodes.Count)];
        }

        private static Node BestChild(Node v)
        {
            var nodes = v.Children.OrderByDescending(n => UBC(n)).ToList();
            if (nodes.Count == 0) return null;
            return nodes[0];
        }

        private static int DefaultPolicy(BoardState vState, bool turn, int depth)
        {
            if (!IsTerminal(vState))
            {
                var actions = (from piece in vState.GetPieces(turn)
                               from move in vState.GetLegalMoves(piece)
                               select (piece, move)).ToList();
                var a = actions[new Random().Next(actions.Count)];
                if (turn) a = new MoveOrdering(Team.RED).Search(vState, depth, null);
                vState.MakeMove(a.piece, a.move);
            }

            var reward = BoardEvaluator(vState);
            return reward;
        }

        private void BackUp(Node v, int reward)
        {
            var invertReward = Player == Team.RED ? 1 : -1;
            reward *= invertReward;
            while (v != null)
            {
                v.Visits += 1;
                v.TotalScore += reward;
                v = v.Parent;
            }
        }

        private static double UBC(Node node)
        {
            var w = node.TotalScore;
            var n = node.Visits;
            var v = w / n;
            var N = node.Parent.Visits;
            return v + Math.Sqrt((2 * Math.Log(N)) / n);
        }

        private static bool IsTerminal(BoardState state) => RedWins(state) || BlackWins(state);

        private static bool RedWins(BoardState state)
        {
            for (int i = 0; i <= (int)Rule.FB_CASTLE; ++i)
                for (int j = (int)Rule.L_CASTLE; j <= (int)Rule.R_CASTLE; ++j)
                    if (state[i, j] == (int)Pieces.B_General)
                        return false;

            return true;
        }

        private static bool BlackWins(BoardState state)
        {
            for (int i = (int)Rule.FR_CASTLE; i <= (int)Rule.COL; ++i)
                for (int j = (int)Rule.L_CASTLE; j <= (int)Rule.R_CASTLE; ++j)
                    if (state[i, j] == (int)Pieces.R_General)
                        return false;

            return true;
        }

        private static int BoardEvaluator(BoardState board)
        {
            if (RedWins(board)) return 50000;
            if (BlackWins(board)) return -50000;
            var score = 0;
            for (int i = 0; i < (int)Rule.ROW; ++i)
                for (int j = 0; j < (int)Rule.COL; ++j)
                    if (board[i, j] != 0)
                        score += board[i, j] + board.PosVal(i, j);

            return score;
        }
    }
}
