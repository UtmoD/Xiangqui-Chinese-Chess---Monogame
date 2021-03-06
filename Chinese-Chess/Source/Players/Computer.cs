﻿using ChineseChess.Source.AI;
using ChineseChess.Source.AI.Minimax;
using ChineseChess.Source.GameObjects.Chess;
using ChineseChess.Source.GameRule;
using ChineseChess.Source.Helper;
using ChineseChess.Source.Main;
using Microsoft.Xna.Framework;
using System;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Threading.Tasks;

namespace ChineseChess.Source.Players
{
    public class Computer : Player
    {
        private bool _isCalculating = false;

        private bool _gameOver = false;

        private int _repeatMoveCount = 0;

        private (Point, Point) _move = (Point.Zero, Point.Zero);

        private (Point, Point) _prevMove = (Point.Zero, Point.Zero);

        private ComplexityMeasuring _moveInfo;


        public IMoveStrategy AIAgent { get; set; }
        public int Depth { get; set; }


        public Computer(IMoveStrategy agent, int depth)
        {
            Tag = PlayerTag.COM;
            AIAgent = agent;
            Depth = depth;
        }



        public override void Update(BoardState board, GameTime gameTime)
        {
            if (_gameOver) OnGameOver();
            else
            {
                if (_move == (Point.Zero, Point.Zero))
                {
                    if (!_isCalculating)
                    {
                        _isCalculating = true;
                        var newBoard = board.Clone();
                        ((Action<BoardState, GameTime>)AsyncCalculation).BeginInvoke(newBoard, gameTime, null, this);
                    }
                }
                else MoveTransition();
            }
            
        }

        private void MoveTransition()
        {
            var focusingPiece = Pieces.Where(p => p.Index == _move.Item1)
                                    .SingleOrDefault();

            var epsilon = 50;
            if (Vector2.Distance(focusingPiece.Position.Round(), _move.Item2.ToPosition()) > epsilon)
            {
                focusingPiece.Position = Vector2.Lerp(focusingPiece.Position, _move.Item2.ToPosition(), 0.025f);
                focusingPiece.Position = new Vector2((float)Math.Round(focusingPiece.Position.X),
                                                  (float)Math.Round(focusingPiece.Position.Y));
                focusingPiece.SetBounds();
            }
            else
            {
                focusingPiece.OnFocusing(); // Set focus for ChessBoard function to check if new index is old index or not

                focusingPiece.Position = _move.Item2.ToPosition();  // Normalize Position because vector lerp with speed 0.1f cannot give exact position
                focusingPiece.SetBounds();  // Normalize rectangle bound

                focusingPiece.OnMoving(new PositionTransitionEventArgs(focusingPiece.Index, _move.Item2));  // Set moving for Chessboard to change turn

                focusingPiece.Index = _move.Item2;

                DrawCheck();

                _move = (Point.Zero, Point.Zero);
            }
        }

        private void DrawCheck()
        {
            if (_repeatMoveCount == 3) IsDraw = true;
            if (_move == (_prevMove.Item2, _prevMove.Item1)) ++_repeatMoveCount;
            else _prevMove = _move;
        }

        private void AsyncCalculation(BoardState board, GameTime gameTime)
        {
            _move = CalculateMoveAsync(board, gameTime).Result;
            _isCalculating = false;
        }

        private async Task<(Point, Point)> CalculateMoveAsync(BoardState board, GameTime gameTime)
        {
            var move = (Point.Zero, Point.Zero);
            await Task.Run(() =>
            {
                var watch = Stopwatch.StartNew();
                move = AIAgent.Search(board, Depth, gameTime);
                watch.Stop();
                if (move == (Point.Zero, Point.Zero)) _gameOver = true;
                //if (AIAgent.Name == "SimpleUCTSearch")
                //{
                //    _moveInfo = new ComplexityMeasuring(AIAgent.PositionsEvaluated, watch.ElapsedMilliseconds);
                //    WriteReport("s-uct.txt");
                //}
            }).ConfigureAwait(false);
            return move;
        }

        private void  WriteReport(string fileName)
        {
            using (StreamWriter writer = File.AppendText(fileName))
                writer.WriteLine($"{_moveInfo.PositionsEvaluated},{_moveInfo.MilliSeconds}");
        }
    }
}
