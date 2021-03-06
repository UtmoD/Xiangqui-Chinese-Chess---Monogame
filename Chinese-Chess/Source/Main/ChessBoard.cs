﻿using ChineseChess.Properties;
using ChineseChess.Source.AI.MCTS;
using ChineseChess.Source.AI.Minimax;
using ChineseChess.Source.GameObjects;
using ChineseChess.Source.GameObjects.Chess;
using ChineseChess.Source.GameRule;
using ChineseChess.Source.Helper;
using ChineseChess.Source.Players;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;

namespace ChineseChess.Source.Main
{
    public class ChessBoard : GameModel
    {
        private static ChessBoard _instance;

        private GameState _gameState;

        private int _turn;
        private int _checkMateSide;

        private uint _checkCount;


        private readonly Message[] _messages;

        private readonly Player[] _players;

        private readonly BoardState _matrixBoard;

        private Piece _focusingPiece;

        public int Winner { get; set; } = -1;

        public Board Board { get; private set; }

        public event EventHandler<BoardState> BoardUpdated;



        public static ChessBoard GetInstance(string redPlayer, string blackPlayer, int depth=1)
        {
            if (_instance == null)
            {
                _instance = new ChessBoard(redPlayer, blackPlayer, depth);
            }

            return _instance;
        }


        public ChessBoard(string redPlayer, string blackPlayer, int depth)
        {
            _gameState = GameState.IDLE;
            _checkCount = 0;

            //_turn = new Random().Next(0, 2);
            _turn = 1;
            _messages = new Message[Enum.GetValues(typeof(GameState)).Length];

            _players = new Player[2];
            _players[(int)Team.BLACK] = PlayerFactory.CreatePlayer(blackPlayer, Team.BLACK, depth);
            _players[(int)Team.RED] = PlayerFactory.CreatePlayer(redPlayer, Team.RED, depth);


            //_players[(int)Team.BLACK] = new Computer(new MinimaxUCT(Team.BLACK), _searchDepth);
            //_players[(int)Team.RED] = new Computer(new MoveOrdering(Team.RED), _searchDepth);
            //_players[(int)Team.RED] = new Human();
            //_players[(int)Team.BLACK] = new Human();
            //_players[(int)Team.RED] = new Computer(new MoveOrdering(Team.RED), 2);

            foreach (var player in _players) player.GameOver += Player_GameOver;
            _matrixBoard = new BoardState();
        }

        private void Player_GameOver(object sender, EventArgs e) => _gameState = GameState.GAMEOVER;

        public void LoadContent(ContentManager contentManager)
        {
            if (contentManager == null) throw new ArgumentNullException(nameof(contentManager));

            for (int i = 0; i < (int)Rule.ROW; ++i)
                for (int j = 0; j < (int)Rule.COL; ++j)
                    if (_matrixBoard[i, j] != 0) PutPieceOnBoard(contentManager, new Point(j, i));

            Board = new Board(contentManager.Load<Texture2D>("board"));
            LoadMessage(contentManager);

            OnBoardUpdating();
        }

        private void LoadMessage(ContentManager contentManager)
        {
            var font = contentManager.Load<SpriteFont>(@"Font\GameEnd");
            var boardCenter = new Point(Board.Width / 2, Board.Height / 2);
            _messages[(int)GameState.B_WIN] = new Message(font, Resources.blackWins, boardCenter);
            _messages[(int)GameState.R_WIN] = new Message(font, Resources.redWins, boardCenter);
            _messages[(int)GameState.CHECKMATE] = new Message(font, Resources.checkMate, boardCenter);
            _messages[(int)GameState.B_TURN] = new Message(font, Resources.blackTurn, boardCenter);
            _messages[(int)GameState.R_TURN] = new Message(font, Resources.redTurn, boardCenter);
            _messages[(int)GameState.DRAW] = new Message(font, Resources.draw, boardCenter);
        }

        private void PutPieceOnBoard(ContentManager contentManager, Point boardIdx)
        {
            var piece = PieceFactory.CreatePiece(_matrixBoard[boardIdx.Y, boardIdx.X],
                                              boardIdx, this,
                                              contentManager);
            piece.Focused += Piece_FocusedHandler;
            piece.Moved += Piece_MovedHandler;
            piece.CheckMated += Piece_CheckMatedHandler;

            if (piece.Value > 0) _players[(int)Team.RED].AddPiece(piece);
            else _players[(int)Team.BLACK].AddPiece(piece);
        }

        private void Piece_CheckMatedHandler(object sender, int e)
        {
            _checkCount++;

            // Quadruple check
            if (_checkCount >= 4) _gameState = GameState.GAMEOVER;
            else
                if (_gameState != GameState.GAMEOVER)
                {
                    _gameState = GameState.CHECKMATE;
                    _checkMateSide = e;
                }
                
        }

        private void Piece_MovedHandler(object sender, PositionTransitionEventArgs e)
        {
            _gameState = GameState.IDLE;

            if (e.NewIdx != _focusingPiece.Index)
            {
                _checkCount = 0;
                _turn = -(_turn) + 1; // switch side

                _messages[(int)GameState.CHECKMATE].ResetTimer();
                _messages[_turn + 3].ResetTimer();
                UpdateBoard(e);
            }
        }


        private void UpdateBoard(PositionTransitionEventArgs e)
        {
            UpdatePieces(e.NewIdx);
            _matrixBoard.MakeMove(e.CurrentIdx, e.NewIdx);
            OnBoardUpdating();
        }

        private void OnBoardUpdating() => BoardUpdated?.Invoke(this, _matrixBoard);

        private void UpdatePieces(Point e)
        {
            // Check if attacking General
            if (Math.Abs(_matrixBoard[e.Y, e.X]) == (int)Pieces.R_General) _gameState = GameState.GAMEOVER;

            _players[_turn].RemovePiece(this, e);
        }

        private void Piece_FocusedHandler(object sender, EventArgs e)
        {
            _gameState = GameState.MOVING;
            _focusingPiece = sender as Piece;
        }


        public override void Update(MouseState mouseState, GameTime gameTime)
        {
            if (_gameState != GameState.GAMEOVER && _gameState != GameState.DRAW)
            {
                CheckMateUpdate();
                if (_gameState == GameState.MOVING) _focusingPiece.Update(mouseState, gameTime);
                else if (_players.Count(p => p.IsDraw) == 2) _gameState = GameState.DRAW;
                else UpdatePiecesInTurn(mouseState, gameTime);
            }

        }

        private void CheckMateUpdate()
        {
            if (_gameState == GameState.CHECKMATE) _messages[(int)_gameState].Update();
        }

        private void UpdatePiecesInTurn(MouseState mouseState, GameTime gameTime)
        {
            _messages[_turn + 3].Update();
            if (_players[_turn].GetType() == typeof(Computer)) _players[_turn].Update(_matrixBoard, gameTime);
            else _players[_turn].Update(mouseState, gameTime);
        }


        public override void Draw(SpriteBatch spriteBatch)
        {
            if (spriteBatch == null) throw new ArgumentNullException(nameof(spriteBatch));

            Board.Draw(spriteBatch);
            DrawPieces(spriteBatch);

            if (_gameState == GameState.GAMEOVER) DrawGameOverMessage(spriteBatch);
            else if (_gameState == GameState.CHECKMATE) DrawCheckMateMessage(spriteBatch);
            else if (_gameState == GameState.DRAW) DrawDrawMessage(spriteBatch);
            else DrawTurnMessage(spriteBatch);
        }

        private void DrawDrawMessage(SpriteBatch spriteBatch)
        {
            var color = Color.Blue;
            _messages[(int)GameState.DRAW].DrawString(spriteBatch, color);
            Winner = 2;
        }

        private void DrawTurnMessage(SpriteBatch spriteBatch)
        {
            var color = Color.Red;
            if (_turn + 3 == (int)GameState.B_TURN) color = Color.Black;

            _messages[_turn + 3].DrawString(spriteBatch, color);
        }

        private void DrawPieces(SpriteBatch spriteBatch)
        {
            foreach (var player in _players) player.DrawPieces(spriteBatch);
        }

        private void DrawCheckMateMessage(SpriteBatch spriteBatch)
        {
            var color = Color.Red;
            if (_checkMateSide < 0) color = Color.Black;

            _messages[(int)_gameState].DrawString(spriteBatch, color);
        }

        private void DrawGameOverMessage(SpriteBatch spriteBatch)
        {
            if (_focusingPiece.Value < 0)
            {
                _messages[(int)GameState.B_WIN].DrawString(spriteBatch, Color.Black);
                Winner = 0;
            }
            else
            {
                _messages[(int)GameState.R_WIN].DrawString(spriteBatch, Color.Red);
                Winner = 1;
            }
                
        }
    }
}
