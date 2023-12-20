using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace _2048_MS_Graph
{/// Auteur: Marc Schilter
/// 2048 en Windows Forms
/// CPNV - SI-CA1a - 2023
    public partial class Form1 : Form
    {
        private int[,] board = new int[4, 4];
        private static bool[,] fusionTuile = new bool[4, 4];
        private int score = 0;
        private Timer animationTimer;
        private Dictionary<Point, Point> DepTuile; // Pour gérer les animations 
        private List<Tuple<Point, Point>> mouvementTuile = new List<Tuple<Point, Point>>();
        public Form1()
        {
            InitializeComponent();
            InitializeGame();  // démarre la partie

            //Double buffering pour éviter que la tableau ne se redessine inutilement.
            this.SetStyle(ControlStyles.DoubleBuffer |
                 ControlStyles.UserPaint |
                 ControlStyles.AllPaintingInWmPaint,
                 true);
            this.UpdateStyles();

            // Activer le double buffering pour le panel
            typeof(Panel).InvokeMember("DoubleBuffered",
                System.Reflection.BindingFlags.SetProperty
                | System.Reflection.BindingFlags.Instance
                | System.Reflection.BindingFlags.NonPublic,
                null, panelJeu, new object[] { true });

            // Configuration du timer pour l'animation
            animationTimer = new Timer();
            animationTimer.Interval = 32;
            animationTimer.Tick += AnimationTick;
            DepTuile = new Dictionary<Point, Point>();
            //Utilisé pour le dessin de la grille et des tuiles dans le panel windows form. 
            panelJeu.Paint += new PaintEventHandler(panelJeu_Paint);
        }
        /****************************************************************************************************************************************
                                        Fonctions utilisées pour les mécaniques et logiques de jeu du 2048. 
        ****************************************************************************************************************************************/

        //InitializeGame initialise la partie en mettant le score à 0 et en générant 2 tuiles aléatoire de valeur 2 ou 4.
        private void InitializeGame()
        {
            // Réinitialiser le score
            score = 0;
            // Initialiser le plateau de jeu avec des zéros
            for (int i = 0; i < board.GetLength(0); i++)
            {
                for (int j = 0; j < board.GetLength(1); j++)
                {
                    board[i, j] = 0;
                }
            }
            // Boucle pour appeler deux fois la fonction RandomTuile()
            for (int i = 0; i < 2; i++)
            {
                RandomTuile();
            }
            // Dessiner la grille initiale          
            panelJeu.Invalidate();
            MajScore();
        }
        //PartieGagne gère la fin de partie dans le cas d'une victoire.
        private bool PartieGagne()
        {
            for (int i = 0; i < board.GetLength(0); i++)
            {
                for (int j = 0; j < board.GetLength(1); j++)
                {
                    if (board[i, j] == 2048)
                    {
                        var continuer = MessageBox.Show("Félicitations! Vous avez atteint la tuile 2048. Voulez-vous continuer?",
                                              "Partie Gagnée",
                                              MessageBoxButtons.YesNo,
                                              MessageBoxIcon.Question);
                        if (continuer == DialogResult.No)
                        {
                            // Code pour terminer le jeu 
                            this.Close(); // Pour fermer le formulaire
                            return true;
                        }
                        return false; //l'utilisateur continue à jouer.
                    }
                }
            }
            return false;
        }
        //PartieFini gère la fin de partie dans le cas d'une défaite.
        private bool PartieFini()
        {
            // Vérifiez d'abord s'il y a des tuiles vides
            for (int i = 0; i < board.GetLength(0); i++)
            {
                for (int j = 0; j < board.GetLength(1); j++)
                {
                    if (board[i, j] == 0)
                    {
                        return false;
                    }
                }
            }
            // Vérifiez ensuite s'il y a des mouvements possibles
            for (int i = 0; i < board.GetLength(0); i++)
            {
                for (int j = 0; j < board.GetLength(1); j++)
                {
                    if ((i - 1 >= 0 && board[i, j] == board[i - 1, j]) ||
                        (i + 1 < 4 && board[i, j] == board[i + 1, j]) ||
                        (j - 1 >= 0 && board[i, j] == board[i, j - 1]) ||
                        (j + 1 < 4 && board[i, j] == board[i, j + 1]))
                    {
                        return false;
                    }
                }
            }
            // Si la partie est terminée
            MessageBox.Show("Game Over! Votre score: " + score, "Partie Terminée", MessageBoxButtons.OK, MessageBoxIcon.Information);
            var recommencer = MessageBox.Show("Voulez-vous recommencer une nouvelle partie?",
                                              "Nouvelle Partie",
                                              MessageBoxButtons.YesNo,
                                              MessageBoxIcon.Question);
            if (recommencer == DialogResult.Yes)
            {
                InitializeGame();
                return false; // La partie est réinitialisée, donc pas vraiment "finie"
            }
            else
            {
                this.Close(); // Ferme l'application
                return true;
            }
        }
        //OnKeyDown permet de gérer les mouvement à l'aide des flèches directionnelles. 
        protected override void OnKeyDown(KeyEventArgs e)
        {
            base.OnKeyDown(e);
            bool moved = false;

            if (e.KeyCode == Keys.Up) moved = DeplaceTuile(0, -1);
            else if (e.KeyCode == Keys.Down) moved = DeplaceTuile(0, 1);
            else if (e.KeyCode == Keys.Left) moved = DeplaceTuile(-1, 0);
            else if (e.KeyCode == Keys.Right) moved = DeplaceTuile(1, 0);

            if (moved)
            {
                RandomTuile();
                // Mettre à jour l'affichage après le mouvement
                panelJeu.Invalidate();
                if (PartieGagne())
                {
                    // L'utilisateur a choisi de ne pas continuer après avoir gagné
                    this.Close();
                }
                else if (PartieFini())
                {
                    // La fonction PartieFini gère déjà la réinitialisation ou la fermeture
                }

            }
        }
        //RandomTuile s'occupe de la génération de tuile aléatoire
        private void RandomTuile()
        {
            // Tableau pour stocker les indices des cellules vides dans la grille
            List<Tuple<int, int>> caseVide = new List<Tuple<int, int>>();

            // Parcours de la grille pour trouver toutes les cellules vides
            for (int i = 0; i < board.GetLength(0); i++)
            {
                for (int j = 0; j < board.GetLength(1); j++)
                {
                    if (board[i, j] == 0) // Si la cellule est vide (contient la valeur 0)
                    {
                        caseVide.Add(new Tuple<int, int>(i, j));
                    }
                }
            }
            // Vérifier s'il y a des cellules vides disponibles
            if (caseVide.Count > 0)
            {
                Random random = new Random();
                // Générer un index aléatoire parmi les cellules vides
                int randomIndex = random.Next(caseVide.Count);
                // Récupérer l'indice de la cellule sélectionnée aléatoirement
                var cell = caseVide[randomIndex];
                int row = cell.Item1;
                int col = cell.Item2;
                // Ajouter une nouvelle tuile (2 ou 4) à la cellule sélectionnée
                board[row, col] = (random.Next(2) + 1) * 2; // Soit 2, soit 4
            }
        }
        //DeplaceTuile est la fonction principale du jeu. Elle gère les mouvements des tuiles, leurs fusions et l'incréementation du score. 
        private bool DeplaceTuile(int dh, int dv)
        {
            bool moved = false;
            mouvementTuile.Clear(); // Réinitialiser les mouvements de tuiles pour ce tour

            // Réinitialiser le tableau de fusion à chaque mouvement
            for (int i = 0; i < board.GetLength(0); i++)
            {
                for (int j = 0; j < board.GetLength(1); j++)
                {
                    fusionTuile[i, j] = false;
                }
            }
            // Déterminer la direction du mouvement
            int[] order = new int[4] { 0, 1, 2, 3 };
            if (dh == 1 || dv == 1)
            {
                order = new int[4] { 3, 2, 1, 0 }; // Inverser l'ordre pour les mouvements vers la droite ou le bas
            }
            // Parcourir la grille
            for (int x = 0; x < board.GetLength(0); x++)
            {
                for (int y = 0; y < board.GetLength(1); y++)
                {
                    int i = order[x];
                    int j = order[y];

                    if (board[i, j] == 0) continue; // Ignorer les cases vides

                    // Trouver la prochaine position
                    int nextI = i + dv;
                    int nextJ = j + dh;

                    while (nextI >= 0 && nextI < 4 && nextJ >= 0 && nextJ < 4)
                    {
                        // Vérifier si la case est occupée
                        if (board[nextI, nextJ] != 0)
                        {
                            // Vérifier si une fusion est possible
                            if (board[nextI, nextJ] == board[i, j] && !fusionTuile[nextI, nextJ] && !fusionTuile[i, j])
                            {
                                board[nextI, nextJ] *= 2;
                                score += board[nextI, nextJ];
                                board[i, j] = 0;
                                fusionTuile[nextI, nextJ] = true;
                                moved = true;
                                mouvementTuile.Add(new Tuple<Point, Point>(new Point(i, j), new Point(nextI, nextJ)));
                                break;
                            }
                            else
                            {
                                // Déplacer la tuile sans fusionner
                                if (nextI - dv != i || nextJ - dh != j)
                                {
                                    board[nextI - dv, nextJ - dh] = board[i, j];
                                    if (nextI - dv != i || nextJ - dh != j) board[i, j] = 0;
                                    moved = true;
                                    mouvementTuile.Add(new Tuple<Point, Point>(new Point(i, j), new Point(nextI - dv, nextJ - dh)));
                                }
                                break;
                            }
                        }
                        nextI += dv;
                        nextJ += dh;
                    }
                    // Gérer le cas où la tuile se déplace vers une case vide en fin de grille
                    if (nextI < 0 || nextI >= 4 || nextJ < 0 || nextJ >= 4)
                    {
                        nextI -= dv;
                        nextJ -= dh;
                        if (nextI != i || nextJ != j)
                        {
                            board[nextI, nextJ] = board[i, j];
                            board[i, j] = 0;
                            moved = true;
                            mouvementTuile.Add(new Tuple<Point, Point>(new Point(i, j), new Point(nextI, nextJ)));
                        }
                    }
                }
            }
            if (moved)
            {
                panelJeu.Invalidate();  // Met à jour la grille
                MajScore(); // Met à jour l'affichage du score
                StartAnimation(); // Démarre l'animation si des tuiles ont bougé
            }
            return moved;
        }
        // MajScore permet de mettre à jour l'affichage du score
        private void MajScore()
        {
            lblScore.Text = $"2048 - Score: {score}";
        }



        /****************************************************************************************************************************************
                    Fonctions utilisées pour le dessin de la grille de jeu dans le Panel de notre Windows Forms. 
        ****************************************************************************************************************************************/

        //DrawGrid dessine de la grille du jeu.
        private void DrawGrid(Graphics g)
        {
            // Taille de chaque cellule de la grille
            int cellSize = 100;
            // Police utilisée pour afficher les numéros dans les tuiles
            Font font = new Font("Arial", 20);
            // Parcourir le tableau 'board' pour dessiner chaque tuile
            for (int i = 0; i < board.GetLength(0); i++) // Parcours des lignes
            {
                for (int j = 0; j < board.GetLength(1); j++) // Parcours des colonnes
                {
                    // Déterminer la couleur de fond de la tuile basée sur sa valeur
                    // 'ColorTuile' est une fonction qui renvoie une couleur spécifique pour chaque valeur de tuile
                    Color color = ColorTuile(board[i, j]);
                    Brush brush = new SolidBrush(color);
                    // Créer un rectangle représentant la tuile à la position (i, j)
                    Rectangle rect = new Rectangle(j * cellSize, i * cellSize, cellSize, cellSize);
                    // Remplir le rectangle avec la couleur déterminée
                    g.FillRectangle(brush, rect);
                    // Dessiner le contour du rectangle (la tuile) avec un stylo noir
                    g.DrawRectangle(Pens.Black, rect);
                    // Déterminer le texte à afficher dans la tuile (la valeur de la tuile, sauf si elle est 0)
                    string text = board[i, j] > 0 ? board[i, j].ToString() : "";
                    // Dessiner le texte au centre du rectangle avec la police et la couleur définies
                    g.DrawString(text, font, Brushes.Black, rect, new StringFormat
                    {
                        Alignment = StringAlignment.Center,
                        LineAlignment = StringAlignment.Center
                    });
                }
            }
        }
        // panelJeul_Paint intégre la fonction de dession de la grille dans le panel.
        private void panelJeu_Paint(object sender, PaintEventArgs e)
        {
            DrawGrid(e.Graphics);
        }
        //ColorTuile assure la gestions des couleurs en fonction de la valeur de la tuile.
        private Color ColorTuile(int value)
        {
            switch (value)
            {
                case 2: return Color.LightSalmon;
                case 4: return Color.LightYellow;
                case 8: return Color.LightBlue;
                case 16: return Color.LightSeaGreen;
                case 32: return Color.LightSteelBlue;
                case 64: return Color.Salmon;
                case 128: return Color.Yellow;
                case 256: return Color.Blue;
                case 512: return Color.SeaGreen;
                case 1024: return Color.SteelBlue;
                case 2048: return Color.DarkSalmon;
                case 4096: return Color.DarkOrchid;
                case 8192: return Color.Chartreuse;
                case 16384: return Color.IndianRed;
                default: return Color.White;
            }
        }
        //StartAnimation initialise l'animation des tuiles à la suite d'un mouvement. 



        /****************************************************************************************************************************************
                     Fonctions pour l'animation de la grille de jeu et les calculs de positions de tuiles utilisés pour l'anmiation. 
        ****************************************************************************************************************************************/

        private void StartAnimation()
        {
            // Vider le dictionnaire 'DepTuile' avant de commencer une nouvelle animation.
            // 'DepTuile' contient les informations sur les tuiles en mouvement (leur position de départ et de fin).
            DepTuile.Clear();

            // Parcourir la liste 'mouvementTuile', qui contient les déplacements de tuiles à animer.
            foreach (var movement in mouvementTuile)
            {
                // 'start' est la position actuelle de la tuile, et 'end' est sa destination.
                var start = movement.Item1; // Position de départ de la tuile.
                var end = movement.Item2;   // Position de fin de la tuile.

                // Mémorise dans 'DepTuile' la destination de chaque tuile.
                // La clé est la position de départ, et la valeur est la position de fin.
                DepTuile[start] = end;
            }

            // Vérifier si le dictionnaire 'DepTuile' contient des tuiles à animer.
            // Si c'est le cas, démarrer le timer qui gère l'animation.
            if (DepTuile.Count > 0)
            {
                animationTimer.Start();
            }
        }
        //AnimationTick actionne le calcul de la position des Tuile grâce é MajDepTuile et redessine le tableau dans le Panel. 
        private void AnimationTick(object sender, EventArgs e)
        {
            // Mise à jour de la position des tuiles en mouvement
            MajDepTuile();

            // Demander le redessinage du panel pour refléter les nouvelles positions des tuiles
            panelJeu.Invalidate();
        }
        //MajDepTuile gère la mise à jour des positions des tuiles pendant l'animation.
        private void MajDepTuile()
        {
            // Liste pour enregistrer les tuiles qui ont atteint leur destination finale.
            List<Point> DepFini = new List<Point>();

            // Dictionnaire temporaire pour conserver les nouvelles positions calculées des tuiles.
            var nouvelPosi = new Dictionary<Point, Point>();

            // Parcourir chaque paire de positions de départ et de fin dans DepTuile.
            foreach (var item in DepTuile)
            {
                // 'start' est la position actuelle de la tuile.
                Point start = item.Key;
                // 'end' est la position finale souhaitée de la tuile.
                Point end = item.Value;

                // Calculer la nouvelle position intermédiaire de la tuile.
                // Utilise la fonction DeplaceVers pour déplacer la tuile d'un pas vers sa destination.
                Point current = new Point(
                    DeplaceVers(start.X, end.X, 100), // Déplacement horizontal.
                    DeplaceVers(start.Y, end.Y, 100)); // Déplacement vertical.

                // Vérifier si la tuile a atteint sa destination.
                if (current == end)
                {
                    // Si la tuile est arrivée à destination, l'ajouter à la liste des déplacements terminés.
                    DepFini.Add(start);
                }
                else
                {
                    // Si la tuile n'est pas encore arrivée, mettre à jour sa nouvelle position dans 'nouvelPosi'.
                    nouvelPosi[start] = current;
                }
            }
            // Mettre à jour les positions dans 'DepTuile' avec les nouvelles positions calculées.
            foreach (var position in nouvelPosi)
            {
                DepTuile[position.Key] = position.Value;
            }
            // Supprimer les tuiles qui ont atteint leur destination du dictionnaire 'DepTuile'.
            foreach (var Fini in DepFini)
            {
                DepTuile.Remove(Fini);
            }
            // Si toutes les tuiles ont atteint leur destination (aucun déplacement restant),arrêter le timer d'animation.
            if (DepTuile.Count == 0)
            {
                animationTimer.Stop();
            }
        }
        //DeplaceVers calcule la position intermédiaire d'une tuile en déplacement pour l'animation.
        private int DeplaceVers(int start, int end, int step)
        {
            // La valeur de 'step' est réduite pour ralentir l'animation. En réduisant 'step', le mouvement de la tuile est plus lent.
            // 'step' détermine la distance que la tuile parcourt à chaque mise à jour de l'animation.
            // 'Math.Max' s'assure que 'step' ne descend pas en dessous de 1 pour éviter une stagnation.
            step = Math.Max(1, step / 1000);
            // Si la position de départ (start) est inférieure à la position de fin (end),
            // cela signifie que la tuile doit se déplacer vers le haut ou vers la droite.
            if (start < end)
            {
                // 'Math.Min' est utilisé pour s'assurer que la tuile ne dépasse pas sa destination.
                // La nouvelle position est la position actuelle plus 'step', mais pas plus que la position de fin.
                return Math.Min(start + step, end);
            }
            // Si la position de départ est supérieure à la position de fin,
            // cela signifie que la tuile doit se déplacer vers le bas ou vers la gauche.
            else if (start > end)
            {
                // 'Math.Max' est utilisé pour s'assurer que la tuile ne recule pas au-delà de sa destination.
                // La nouvelle position est la position actuelle moins 'step', mais pas moins que la position de fin.
                return Math.Max(start - step, end);
            }
            // Si la position de départ est égale à la position de fin, cela signifie que la tuile est déjà à sa destination. 
            // Dans ce cas, retourne simplement la position de fin.
            else
            {
                return end;
            }
        }

    }
}
