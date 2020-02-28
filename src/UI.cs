using System;
using System.Linq;
using System.Threading;
using System.Collections.Generic;
using SDL2;

namespace skaktego {

    public sealed class UI : IPlayer {
        // Screen size
        private const int SCREEN_WIDTH = 800;
        private const int SCREEN_HEIGHT = 450;

        private const string RESOURCE_PATH = "resources/";

        private static UI instance = null;
        private static readonly object padlock = new object();

        private Window window;
        private Renderer renderer;
        private Font font;
        private Texture background;
        private Texture menuLogo;
        private Texture menuBG;
        private Texture aiCheckmark;
        private Texture pieceSprites;
        private Texture[] endTextTextures;
        private Texture overlayText1;
        private Texture overlayText2;
        private Texture skaktegoPrepText1;
        private Texture skaktegoPrepText2;
        private Texture menuAIText;
        private Rect[,] pieceClips;
        private Nullable<BoardPosition> highlightedTile = null;
        private Nullable<BoardPosition> selectedTile = null;
        private List<BoardPosition> legalMoves;
        private List<SDL.SDL_Event> events;
        private Rect screenRect = null;
        private bool isMenuActive = false;
        private bool isGaming = false;
        private bool doneGaming = false;
        private bool screenHidden = false;
        private bool aiPlaying = true;
        //Has the user already pressed something this frame
        private bool pressedSomething = false;
        // The color of the last move's player
        private ChessColors lastPlayer = ChessColors.White;
        // The color of the current player
        private ChessColors playerColor = ChessColors.White;
        private Button[] buttons;
        private Button[] menuButtons;
        private Button[] gameButtons;
        private Button[] endButtons;
        private GameResults gameResult = GameResults.Tie;

        private MVar<ChessMove> storedMove;
        private GameState gameState = null;
        private Game game = null;
        private Thread gameThread;

        public bool quit = false;

        UI() {
            Graphics.InitGraphics();

            window = new Window("skaktego", 100, 100, SCREEN_WIDTH, SCREEN_HEIGHT);
            renderer = new Renderer(window);

            using (Surface icon = new Surface("skaktegoIcon.png")) {
                window.SetWindowIcon(icon);
            }

            font = new Font("playfair-display/PlayfairDisplay-Regular.ttf", 128);

            legalMoves = new List<BoardPosition>();

            events = new List<SDL.SDL_Event>();

            buttons = new Button[]{
                new Button(3/8.0, 8/24.0, 1/4.0, 1/12.0, "Fortsæt", font, () => isMenuActive = false),
                new Button(3/8.0, 11/24.0, 1/4.0, 1/12.0, "Nyt Spil", font, () => {
                    GameTypes gameType = gameState.gameType == GameTypes.Normal ? GameTypes.Normal : GameTypes.SkaktegoPrep;
                    StopGaming();
                    BeginGaming(gameType);
                }),
                new Button(3/8.0, 14/24.0, 1/4.0, 1/12.0, "Hovedmenu", font, StopGaming)
            };

            menuButtons = new Button[]{
                new Button(3/8.0,  9/24.0, 1/3.0, 1/12.0, "Spil Skaktego", font, () => {
                    BeginGaming(GameTypes.SkaktegoPrep);
                }),
                new Button(3/8.0, 12/24.0, 1/3.0, 1/12.0, " Spil Skak ", font, () => {
                    BeginGaming(GameTypes.Normal);
                }),
                new Button(3/8.0, 15/24.0, 1/3.0, 1/12.0, "     Luk     ", font, () => quit = true),
                new Button(1/4.0, 12/24.0, 1/12.0, 1/12.0, " ", font, () => aiPlaying = !aiPlaying)
            };

            gameButtons = new Button[]{
                new Button(0, 0, 1, 1, "X", font, () => isMenuActive = true)
            };

            endButtons = new Button[]{
                new Button(3/12.0, 14/24.0, 1/6.0, 1/12.0, "Omkamp", font, () => {
                    GameTypes gameType = gameState.gameType == GameTypes.Normal ? GameTypes.Normal : GameTypes.SkaktegoPrep;
                    StopGaming();
                    BeginGaming(gameType);
                }),
                new Button(7/12.0, 14/24.0, 1/6.0, 1/12.0, "Hovedmenu", font, StopGaming)
            };

            background = new Texture(renderer, "background.png");
            menuLogo = new Texture(renderer, "skaktegoLogo.png");
            menuBG = new Texture(renderer, "skaktegoMain.png");
            aiCheckmark = new Texture(renderer, "checkmark.png");
            pieceSprites = new Texture(renderer, "pieces.png");
            pieceClips = UI.GetPieceClips(pieceSprites);

            Color[] endColors = new Color[] { Graphics.white, Graphics.black, Graphics.gray };
            string[] endTexts = new string[] { "Hvid Vinder", "Sort Vinder", " Uafgjort " };
            endTextTextures = new Texture[3];
            for (int i = 0; i < endTexts.Length; i++) {
                using (Surface textSurf = font.TextSurface(endTexts[i], endColors[i])) {
                    endTextTextures[i] = new Texture(renderer, textSurf);
                }
            }

            using (Surface textSurf = font.TextSurface("Næste Tur", Graphics.white)) {
                overlayText1 = new Texture(renderer, textSurf);
            }
            using (Surface textSurf = font.TextSurface("Tryk en knap for at fortsætte", Graphics.white)) {
                overlayText2 = new Texture(renderer, textSurf);
            }
            using (Surface textSurf = font.TextSurface("Skaktego forberedelsesfase. Tryk på din egne brikker", Graphics.white)) {
                skaktegoPrepText1 = new Texture(renderer, textSurf);
            }
            using (Surface textSurf = font.TextSurface("for at skifte deres plads, tryk enter når du er færdig.", Graphics.white)) {
                skaktegoPrepText2 = new Texture(renderer, textSurf);
            }
            using (Surface textSurf = font.TextSurface("AI", Graphics.black)) {
                menuAIText = new Texture(renderer, textSurf);
            }
        }

        public void SetGameState(GameState gameState) {
            this.gameState = gameState;
        }

        public ChessMove GetMove(GameState gameState, ChessColors color) {
            this.gameState = gameState;
            this.playerColor = color;
            return storedMove.Var;
        }

        private void BeginGaming(GameTypes gameType) {
            isGaming = true;
            isMenuActive = false;
            screenHidden = false;
            doneGaming = false;
            highlightedTile = null;
            selectedTile = null;
            legalMoves.Clear();

            storedMove = new MVar<ChessMove>();

            IPlayer whitePlayer = this;
            IPlayer blackPlayer;
            // Choose the second player.
            if (aiPlaying) {
                // Normal chess uses more computational power
                if (gameType == GameTypes.Normal) {
                    blackPlayer = new ChessAI(3);
                } else {
                    blackPlayer = new ChessAI(4);
                }
            } else {
                blackPlayer = this;
            }

            game = new Game(whitePlayer, blackPlayer, gameType);
            gameThread = new Thread(new ThreadStart(() => {
                Tuple<GameState, GameResults> results = game.PlayGame();
                gameState = results.Item1;
                gameResult = results.Item2;
                doneGaming = true;
            }));
            gameThread.Start();
        }

        private void StopGaming() {
            isGaming = false;
            highlightedTile = null;
            selectedTile = null;
            legalMoves.Clear();
            if (game != null) {
                game.quit = true;
            }
            if (storedMove != null && !storedMove.HasValue) {
                storedMove.Var = new ChessMove();
            }
            if (gameThread != null && gameThread.IsAlive) {
                gameThread.Join();
            }
        }

        public void Update() {
            PollEvents();

            // When the player switches, hide the screen
            if (gameState != null &&
                gameState.player != lastPlayer &&
                gameState.gameType != GameTypes.Normal &&
                !aiPlaying
            ) {
                screenHidden = true;
                lastPlayer = gameState.player;
            }

            // Update screen rect
            screenRect = renderer.OutputRect();

            //Reset if the user has already pressed a button this frame
            pressedSomething = false;

            // Check if the user wants to quit.
            if (events.Any(e => e.type == SDL.SDL_EventType.SDL_QUIT)) {
                quit = true;
            }


            if (isGaming) {
                if (gameState != null) {
                    UpdateGame();
                }
            } else {
                UpdateMainMenu();
            }

        }

        private void UpdateGame() {
            // If the user presses a button, unhide the screen
            if (events.Any(e => e.type == SDL.SDL_EventType.SDL_KEYUP ||
            e.type == SDL.SDL_EventType.SDL_MOUSEBUTTONUP) &&
            screenHidden &&
            !pressedSomething) {
                screenHidden = false;
                pressedSomething = true;
            }

            // Check if the user is done preparing in skaktego
            if (gameState.gameType == GameTypes.SkaktegoPrep &&
            events.Any(e => e.type == SDL.SDL_EventType.SDL_KEYUP &&
            e.key.keysym.sym == SDL.SDL_Keycode.SDLK_RETURN) &&
            !pressedSomething) {

                if (storedMove.HasValue) {
                    storedMove.TakeMVar(x => x);
                }
                storedMove.Var = Game.DONE_PREPARING_MOVE;

                pressedSomething = true;
            }

            int mouseX, mouseY;
            SDL.SDL_GetMouseState(out mouseX, out mouseY);

            if (events.Any(e => e.type == SDL.SDL_EventType.SDL_KEYUP &&
            e.key.keysym.sym == SDL.SDL_Keycode.SDLK_ESCAPE) &&
            !pressedSomething) {
                isMenuActive = !isMenuActive;
                pressedSomething = true;
            }

            Rect boardRect = new Rect(0, 0, 0, 0);
            {
                // Board size
                int bs = gameState.board.Size;
                boardRect.W = Math.Min(screenRect.H / bs * bs, screenRect.W / bs * bs);
                boardRect.H = Math.Min(screenRect.H / bs * bs, screenRect.W / bs * bs);
            }

            boardRect.X = (int)Math.Round((screenRect.W - boardRect.W) * 0.5);
            boardRect.Y = (int)Math.Round((screenRect.H - boardRect.H) * 0.5);

            int boardMouseX = (int)Math.Floor((mouseX - boardRect.X) / (double)boardRect.W * gameState.board.Size);
            int boardMouseY = -1 + gameState.board.Size - ((int)Math.Floor((mouseY - boardRect.Y) / (double)boardRect.H * gameState.board.Size));

            if (isMenuActive) {
                foreach (Button button in buttons) {
                    if (pressedSomething) {
                        break;
                    }
                    pressedSomething |= button.Update(mouseX, mouseY, boardRect, events);
                }
            } else if (!screenHidden) {
                Rect xButtonRect = new Rect(0, 0, 0, 0);
                xButtonRect.W = (Math.Min(screenRect.H, screenRect.W) / 16);
                xButtonRect.H = (Math.Min(screenRect.H, screenRect.W) / 16);

                xButtonRect.X = screenRect.W - (Math.Min(screenRect.H, screenRect.W) / 12);
                xButtonRect.Y = (Math.Min(screenRect.H, screenRect.W) / 50);
                foreach (Button button in gameButtons) {
                    if (pressedSomething) {
                        break;
                    }
                    pressedSomething |= button.Update(mouseX, mouseY, xButtonRect, events);
                }

                if (!doneGaming) {
                    if (0 <= boardMouseX && boardMouseX < gameState.board.Size) {
                        if (0 <= boardMouseY && boardMouseY < gameState.board.Size) {
                            highlightedTile = new BoardPosition(boardMouseX, boardMouseY);
                        } else {
                            highlightedTile = null;
                        }
                    } else {
                        highlightedTile = null;
                    }
                    if (events.Any(e => e.type == SDL.SDL_EventType.SDL_MOUSEBUTTONUP &&
                        e.button.button == SDL.SDL_BUTTON_LEFT)                       &&
                        !pressedSomething
                    ) {
                        pressedSomething = true;
                        // If a tile is already selected, attempt to apply the move
                        if (selectedTile.HasValue    &&
                            highlightedTile.HasValue
                        ) {
                            ChessMove move = new ChessMove(selectedTile.Value, highlightedTile.Value);
                            bool isMoveLegal = false;
                            foreach (BoardPosition legalPos in legalMoves) {
                                if (move.to == legalPos) {
                                    isMoveLegal = true;
                                }
                            }
                            if (isMoveLegal) {
                                if (storedMove.HasValue) {
                                    storedMove.TakeMVar(x => x);
                                }
                                storedMove.Var = move;
                                legalMoves.Clear();
                                selectedTile = null;
                                highlightedTile = null;
                            }
                        }
                        SelectTile(gameState, highlightedTile);
                    } else if (events.Any(e => e.type == SDL.SDL_EventType.SDL_MOUSEBUTTONUP &&
                               e.button.button == SDL.SDL_BUTTON_RIGHT)                      &&
                               !pressedSomething
                    ) {
                        selectedTile = null;
                        legalMoves.Clear();
                        pressedSomething = true;
                    }
                }
                if (doneGaming) {
                    foreach (Button button in endButtons) {
                        if (pressedSomething) {
                            break;
                        }
                        pressedSomething |= button.Update(mouseX, mouseY, screenRect, events);
                    }
                }
            }
            DrawGame(gameState);
        }

        private void UpdateMainMenu() {
            int mouseX, mouseY;
            SDL.SDL_GetMouseState(out mouseX, out mouseY);


            Rect mainMenuRect = new Rect(0, 0, 0, 0);
            mainMenuRect.W = Math.Min(screenRect.H, screenRect.W);
            mainMenuRect.H = Math.Min(screenRect.H, screenRect.W);

            mainMenuRect.X = (int)Math.Round((screenRect.W - mainMenuRect.W) * 0.5);
            mainMenuRect.Y = (int)Math.Round((screenRect.H - mainMenuRect.H) * 0.5);

            foreach (Button button in menuButtons) {
                if (pressedSomething) {
                    break;
                }
                pressedSomething |= button.Update(mouseX, mouseY, mainMenuRect, events);
            }

            DrawMainMenu();
        }

        private void SelectTile(GameState gameState, Nullable<BoardPosition> tile) {
            selectedTile = highlightedTile;
            if (selectedTile.HasValue &&
                playerColor == gameState.player
            ) {
                legalMoves = Engine.GetLegalMoves(gameState, selectedTile.Value);
            } else {
                legalMoves.Clear();
            }
        }

        public void DrawGame(GameState gameState) {
            if (screenHidden) {
                DrawOverlay();
                renderer.Present();
                return;
            }


            // Draw the background
            // and clear the screen
            renderer.SetColor(new Color(0X111111));
            renderer.Clear();

            // Screen geometry only during gameplay
            Rect boardRect = new Rect(0, 0, 0, 0);
            {
                // Board size
                int bs = gameState.board.Size;
                boardRect.W = Math.Min(screenRect.H / bs * bs, screenRect.W / bs * bs);
                boardRect.H = Math.Min(screenRect.H / bs * bs, screenRect.W / bs * bs);
            }

            // Set board dimensions
            boardRect.X = (int)Math.Round((screenRect.W - boardRect.W) * 0.5);
            boardRect.Y = (int)Math.Round((screenRect.H - boardRect.H) * 0.5);

            // Draw the graveyard
            DrawGraveyard(screenRect, boardRect);
            
            // Draw the board
            DrawBoard(gameState.board, boardRect);

            // Draw the higlighted, selected and legal tiles
            if (highlightedTile != null) {
                HighlightTile(new Color(0XFFFF0055), gameState.board, boardRect, highlightedTile.Value);
            }
            if (selectedTile != null) {
                HighlightTile(new Color(0X0000FF55), gameState.board, boardRect, selectedTile.Value);
            }
            if (legalMoves != null) {
                foreach (BoardPosition legalTile in legalMoves) {
                    HighlightTile(new Color(0X11FF1155), gameState.board, boardRect, legalTile);
                }
            }


            DrawXButton();

            if (doneGaming) {
                DrawEndScreen(screenRect);
            }

            if (gameState.gameType == GameTypes.SkaktegoPrep) {
                DrawSkaktegoHelp();
            }

            // Draw the in game menu, if it is active
            if (isMenuActive) {
                DrawGameMenu(boardRect);
            }

            renderer.Present();
        }

        private void DrawMainMenu() {
            renderer.SetColor(new Color(0XdfedecFF));
            renderer.FillRect(screenRect);

            Rect mainMenuRect = new Rect(0, 0, 0, 0);
            mainMenuRect.W = Math.Min(screenRect.H, screenRect.W);
            mainMenuRect.H = Math.Min(screenRect.H, screenRect.W);

            mainMenuRect.X = (int)Math.Round((screenRect.W - mainMenuRect.W) * 0.5);
            mainMenuRect.Y = (int)Math.Round((screenRect.H - mainMenuRect.H) * 0.5);

            Rect bgRect = new Rect(0, 0, 0, 0);
            int bgW, bgH;
            menuBG.Query(out bgW, out bgH);
            bgRect.W = (int)(Math.Max(screenRect.H / (double)bgH, screenRect.W / (double)bgW) * bgW);
            bgRect.H = (int)(Math.Max(screenRect.H / (double)bgH, screenRect.W / (double)bgW) * bgH);

            bgRect.X = (screenRect.W - bgRect.W) / 2;
            bgRect.Y = 0;
            renderer.RenderTexture(menuBG, bgRect, null);

            Rect logoRect = new Rect(0, 0, 0, 0);
            int logoW, logoH;
            menuLogo.Query(out logoW, out logoH);
            logoRect.W = (int)((Math.Min(screenRect.H / (double)logoH, screenRect.W / (double)logoW) * logoW) * 0.4);
            logoRect.H = (int)((Math.Min(screenRect.H / (double)logoH, screenRect.W / (double)logoW) * logoH) * 0.4);

            logoRect.X = (int)Math.Round((screenRect.W - logoRect.W) * 0.5);
            logoRect.Y = (int)Math.Round((screenRect.H - logoRect.H) * 0.15);

            renderer.RenderTexture(menuLogo, logoRect, null);

            // Draw the buttons in the main menu
            foreach (Button button in menuButtons) {
                button.Draw(renderer, mainMenuRect);
            }

            Rect aiTextRect = new Rect(
                (int)Math.Round(1/4.0 * mainMenuRect.W + mainMenuRect.X),
                (int)Math.Round(10/24.0 * mainMenuRect.H + mainMenuRect.Y),
                (int)Math.Round(1/12.0 * mainMenuRect.W),
                (int)Math.Round(1/12.0 * mainMenuRect.H));
            renderer.RenderTexture(menuAIText, aiTextRect, null);

            if (aiPlaying) {
                Rect aiRect = new Rect(
                    (int)Math.Round(1/4.0 * mainMenuRect.W + mainMenuRect.X + 0.5 * (1/12.0 * mainMenuRect.H - 1/14.0 * mainMenuRect.H)),
                    (int)Math.Round(12/24.0 * mainMenuRect.H + mainMenuRect.Y + 0.5 * (1/12.0 * mainMenuRect.H - 1/14.0 * mainMenuRect.H)),
                    (int)Math.Round(1/14.0 * mainMenuRect.W),
                    (int)Math.Round(1/14.0 * mainMenuRect.H));
                renderer.RenderTexture(aiCheckmark, aiRect, null);
            }

            renderer.Present();
        }

        private void DrawGameMenu(Rect boardRect) {
            // Draw a dark overlay
            renderer.SetColor(new Color(0X00000077));
            renderer.FillRect(screenRect);

            // Draw the menu rectangle
            Rect menuRect1 = new Rect(0, 0, 11, 11);
            Rect menuRect2 = new Rect(0, 0, 10, 10);

            menuRect1.W = (int)Math.Round(boardRect.W * 0.41);
            menuRect1.H = (int)Math.Round(boardRect.H * 0.51);
            menuRect2.W = (int)Math.Round(boardRect.W * 0.4);
            menuRect2.H = (int)Math.Round(boardRect.H * 0.5);

            menuRect1.X = (int)Math.Round((screenRect.W - menuRect1.W) * 0.5);
            menuRect1.Y = (int)Math.Round((screenRect.H - menuRect1.H) * 0.5);
            menuRect2.X = (int)Math.Round((screenRect.W - menuRect2.W) * 0.5);
            menuRect2.Y = (int)Math.Round((screenRect.H - menuRect2.H) * 0.5);

            renderer.SetColor(new Color(0XFFFFFFAA));
            renderer.FillRect(menuRect1);
            renderer.SetColor(new Color(0X000000DD));
            renderer.FillRect(menuRect2);

            // Draw the buttons in the menu
            foreach (Button button in buttons) {
                button.Draw(renderer, boardRect);
            }
        }

        private void DrawBoard(Board board, Rect dst) {
            // Fill the rectangle with white
            renderer.SetColor(Graphics.white);
            renderer.FillRect(dst);

            // Draw the black squares & pieces
            renderer.SetColor(Graphics.black);
            int x, y;
            int w = (int)Math.Round(dst.W / (double)board.Size);
            int h = (int)Math.Round(dst.H / (double)board.Size);
            for (int i = 0; i < board.Size; i++) {
                y = dst.H - h * i - h + dst.Y;
                for (int j = 0; j < board.Size; j++) {
                    bool fillSquare = (i + j & 1) == 0;
                    x = w * j + dst.X;
                    Rect square = new Rect(x, y, w, h);

                    if (fillSquare) {
                        renderer.FillRect(square);
                    }

                    Piece piece = board.GetPiece(new BoardPosition(j, i));
                    if (piece != null) {
                        DrawPiece(piece, square);
                    }
                }
            }
        }

        private void DrawPiece(Piece piece, Rect dst) {
            Rect clip = pieceClips[(int)piece.Type, (int)piece.Color];
            renderer.RenderTexture(pieceSprites, dst, clip);
        }

        private void DrawXButton() {
            //Draw game button
            Rect xButtonRect = new Rect(0, 0, 0, 0);
            xButtonRect.W = (Math.Min(screenRect.H, screenRect.W) / 16);
            xButtonRect.H = (Math.Min(screenRect.H, screenRect.W) / 16);

            xButtonRect.X = screenRect.W - (Math.Min(screenRect.H, screenRect.W) / 12);
            xButtonRect.Y = (Math.Min(screenRect.H, screenRect.W) / 50);

            Rect xButtonRectOutline = new Rect(0, 0, 0, 0);
            xButtonRectOutline.W = (Math.Min(screenRect.H, screenRect.W) / 15);
            xButtonRectOutline.H = (Math.Min(screenRect.H, screenRect.W) / 15);

            xButtonRectOutline.X = (screenRect.W - (Math.Min(screenRect.H, screenRect.W) / 12)) - ((xButtonRectOutline.W - xButtonRect.W) / 2);
            xButtonRectOutline.Y = (Math.Min(screenRect.H, screenRect.W) / 50) - ((xButtonRectOutline.W - xButtonRect.W) / 2);

            renderer.SetColor(new Color(0XffBBBBBBFF));
            renderer.FillRect(xButtonRectOutline);

            foreach (Button button in gameButtons) {
                button.Draw(renderer, xButtonRect);
            }
            renderer.SetColor(new Color(0Xff002277));
            renderer.FillRect(xButtonRect);
        }

        private void DrawGraveyard(Rect screen, Rect board) {
            int x = 0, y = 0;
            int i = 0, j = 0;
            int w = (int)(Math.Min(screen.H, screen.W) / 12.0);
            int h = w;
            int graveSpace = board.X / w == 0? screen.W / w : board.X / w;
            Rect square = new Rect(x,y,w,h);
            Rect halfScreen = new Rect(0,0,screen.W/2,screen.H);
            if (screenRect.H > screenRect.W) {
                halfScreen.Y = screen.H/2;
                halfScreen.W = screen.W;
                halfScreen.H = screen.H/2;
            }
            renderer.SetColor(new Color(0X222222FF));
            renderer.FillRect(halfScreen);

            for (int k = 0; k < gameState.taken.Count; k++) {
                if (gameState.taken[k].Color == ChessColors.Black) {
                    x = w * (j % graveSpace);
                    y = (screen.H - h) - (h * (j / graveSpace));
                    square.X = x;
                    square.Y = y;
                    DrawPiece(gameState.taken[k], square);
                    j++;
                } else {
                    x = w * (i % graveSpace);
                    y = h * (i / graveSpace);
                    square.X = x;
                    square.Y = y;
                    DrawPiece(gameState.taken[k], square);
                    i++;
                }
            }
        }

        private void DrawEndScreen(Rect dst) {
            if (!doneGaming) {
                return;
            }


            Rect endOverlayRect = new Rect(0, 0, 0, 0);
            endOverlayRect.W = dst.W - dst.W / 6;
            endOverlayRect.H = dst.H - dst.H / 6;
            endOverlayRect.X = (dst.W - endOverlayRect.W) / 2;
            endOverlayRect.Y = (dst.H - endOverlayRect.H) / 2;

            Rect endTextRect = new Rect(0, 0, 0, 0);
            endTextRect.W = (int)(dst.W - dst.W * 0.5);
            endTextRect.H = (int)(dst.H - dst.H * 0.75);
            endTextRect.X = (dst.W - endTextRect.W) / 2;
            endTextRect.Y = (dst.H - endTextRect.H) / 4;

            renderer.SetColor(new Color(0X00000077));
            renderer.FillRect(screenRect);
            renderer.SetColor(new Color(0X55555577));
            renderer.FillRect(endOverlayRect);

            if (gameResult != GameResults.Quit) {
                Texture textTexture = endTextTextures[(int)gameResult];
                renderer.RenderTexture(textTexture, endTextRect, null);
            }

            foreach (Button button in endButtons) {
                button.Draw(renderer, screenRect);
            }
        }

        private void DrawOverlay() {
            renderer.SetColor(new Color(0X000000FF));
            renderer.Clear();

            Rect overlayTextRect1 = new Rect(0, 0, 0, 0);
            overlayTextRect1.W = (int)(screenRect.W - screenRect.W * 0.5);
            overlayTextRect1.H = (int)(screenRect.H - screenRect.H * 0.75);
            overlayTextRect1.X = (screenRect.W - overlayTextRect1.W) / 2;
            overlayTextRect1.Y = (screenRect.H - overlayTextRect1.H) / 4;

            Rect overlayTextRect2 = new Rect(0, 0, 0, 0);
            overlayTextRect2.W = (int)(screenRect.W - screenRect.W * 0.5);
            overlayTextRect2.H = (int)(screenRect.H - screenRect.H * 0.9);
            overlayTextRect2.X = (int)((screenRect.W - overlayTextRect2.W) * 0.5);
            overlayTextRect2.Y = (int)((screenRect.H - overlayTextRect2.H) * 0.6);

            renderer.RenderTexture(overlayText1, overlayTextRect1, null);
            renderer.RenderTexture(overlayText2, overlayTextRect2, null);
        }

        private void DrawSkaktegoHelp() {
            Rect prepText1 = new Rect(0,0,0,0);
            Rect prepText2 = new Rect(0,0,0,0);
            Rect prepOverlay = new Rect(0,0,0,0);

            prepOverlay.X = (int)(screenRect.W * 0.175);
            prepOverlay.Y = (int)(screenRect.H * 0.35);
            prepOverlay.W = (int)(screenRect.W * 0.7);
            prepOverlay.H = (int)(screenRect.H * 0.3);

            prepText1.X = (int)(screenRect.W * 0.2);
            prepText1.Y = (int)(screenRect.H * 0.4);
            prepText1.W = (int)(screenRect.W * 0.65);
            prepText1.H = (int)(screenRect.H * 0.1);

            prepText2.X = (int)(screenRect.W * 0.2);
            prepText2.Y = (int)(screenRect.H * 0.5);
            prepText2.W = (int)(screenRect.W * 0.65);
            prepText2.H = (int)(screenRect.H * 0.1);

            renderer.SetColor(new Color(0X00000099));
            renderer.FillRect(prepOverlay);
            renderer.RenderTexture(skaktegoPrepText1, prepText1, null);
            renderer.RenderTexture(skaktegoPrepText2, prepText2, null);
        }

        private void HighlightTile(Color color, Board board, Rect boardRect, BoardPosition pos) {
            int w = (int)Math.Round(boardRect.W / (double)board.Size);
            int h = (int)Math.Round(boardRect.H / (double)board.Size);
            int x = boardRect.X + pos.column * w;
            int y = boardRect.H + boardRect.Y - pos.row * h - h;

            Rect dst = new Rect(x, y, w, h);

            renderer.SetColor(color);
            renderer.FillRect(dst);
        }

        private void PollEvents() {
            events.Clear();

            SDL.SDL_Event e;
            while (SDL.SDL_PollEvent(out e) != 0) {
                events.Add(e);
            }

        }

        static Rect[,] GetPieceClips(Texture texture) {
            int tW, tH;
            texture.Query(out tW, out tH);
            Rect[,] clips = new Rect[Piece.PIECE_TYPE_COUNT, Piece.PIECE_COLOR_COUNT];

            int w = tW / Piece.PIECE_TYPE_COUNT;
            int h = tH / Piece.PIECE_COLOR_COUNT;
            for (int y = 0; y < Piece.PIECE_COLOR_COUNT; y++) {
                for (int x = 0; x < Piece.PIECE_TYPE_COUNT; x++) {
                    clips[x, y] = new Rect(x * w, y * w, w, h);
                }
            }

            return clips;
        }

        public void Quit() {
            StopGaming();
            Graphics.QuitGraphics();
            instance = null;
        }

        public static UI Instance {
            get {
                lock (padlock) {
                    if (instance == null) {
                        instance = new UI();
                    }
                    return instance;
                }
            }
        }
    }
}
