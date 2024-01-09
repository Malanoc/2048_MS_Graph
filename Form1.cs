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
        private Dictionary<Point, TuileAnimation> tuileAnimations = new Dictionary<Point, TuileAnimation>();
        private bool aCliqueContinuer = false;
        private class TuileAnimation
          {
            public Point Debut { get; set; }
            public Point Fin { get; set; }
            public Point Actuel { get; set; }
            public float Progres { get; set; }
            public int Valeur { get; set; }  
          }

        public Form1()
        {
            InitializeComponent();
            // démarre la partie
            InitialisePartie();
            this.KeyPreview = true;

            // Activer le double buffering pour le panel
            typeof(Panel).InvokeMember("DoubleBuffered",
                System.Reflection.BindingFlags.SetProperty
                | System.Reflection.BindingFlags.Instance
                | System.Reflection.BindingFlags.NonPublic,
                null, panelJeu, new object[] { true });

            // Configuration du timer pour l'animation
            animationTimer = new Timer();
            animationTimer.Interval = 16;
            animationTimer.Tick += AnimationTick;
            DepTuile = new Dictionary<Point, Point>();
            //Utilisé pour le dessin de la grille et des tuiles dans le panel windows form. 
            panelJeu.Paint += new PaintEventHandler(panelJeu_Paint);
        }
        /****************************************************************************************************************************************
                                        Fonctions utilisées pour les mécaniques et logiques de jeu 2048. 
        ****************************************************************************************************************************************/

        /// <summary>
        /// Initialise la partie en mettant le score à 0 et en générant 2 tuiles aléatoire de valeur 2 ou 4.
        /// </summary>
        private void InitialisePartie()
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
        /// <summary>
        /// Gère la fin de partie dans le cas d'une victoire.
        /// </summary>
        /// <returns></returns>
        private bool PartieGagne()
        {
            //Prend en compte que l'utilisateur a déjà atteint 2048 durant la partie et veut continuer à jouer.
            if (!aCliqueContinuer)
            {
                //Recherche une tuile 2048 dans le board.
                for (int i = 0; i < board.GetLength(0); i++)
                {
                    for (int j = 0; j < board.GetLength(1); j++)
                    {
                        if (board[i, j] == 2048)
                        {
                            //Boite de dialogue pour proposer à l'utilisateur de continuer la partie après avoir créé une tuile 2048.
                            var continuer = MessageBox.Show("Félicitations! Vous avez atteint la tuile 2048. Voulez-vous continuer?",
                                                "Partie Gagnée",
                                                MessageBoxButtons.YesNo,
                                                MessageBoxIcon.Question);
                            if (continuer == DialogResult.No)
                            {
                                // Code pour terminer le jeu et fermer le programme.
                                this.Close(); 
                                return true;
                            }
                            //L'utilisateur continue à jouer après avoir vu la message box de victoire. 
                            aCliqueContinuer = true;
                            return false;

                        }

                    }
                }
            }
            //L'utilisateur continue à jouer.
            return false;
        }
        /// <summary>
        /// Gère la fin de partie dans le cas d'une défaite.
        /// </summary>
        /// <returns></returns>
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
                // La partie est réinitialisée, donc pas vraiment "finie"
                aCliqueContinuer = false;
                InitialisePartie();               
                return false;
            }
            else
            {
                // Ferme l'application
                this.Close(); 
                return true;
            }
        }
        /// <summary>
        /// OnKeyDown permet de gérer les mouvement à l'aide des flèches directionnelles. 
        /// </summary>
        /// <param name="e">La touche qui a été appuyée</param>
        protected override void OnKeyUp(KeyEventArgs e)
        {
            base.OnKeyUp(e);
            bool aBouge = false;
            //détermine dans quelle direction l'utilisateur veut bouger les tuiles en utilisant les touches directionnelles
            if (e.KeyCode == Keys.Up) aBouge = DeplaceTuile(0, -1);
            else if (e.KeyCode == Keys.Down) aBouge = DeplaceTuile(0, 1);
            else if (e.KeyCode == Keys.Left) aBouge = DeplaceTuile(-1, 0);
            else if (e.KeyCode == Keys.Right) aBouge = DeplaceTuile(1, 0);
            else if (e.KeyCode == Keys.C) this.Close();
            else if (e.KeyCode == Keys.I) MessageBox.Show("Comment Jouer?\n" + "Utilisez les flèches du clavier pour déplacer les tuiles.\n"+
                                                          "Quand deux tuiles avec le même nombre se touchent, elles fusionnent !");
            if (aBouge)
            {
                // Mettre à jour l'affichage après le mouvement
                panelJeu.Invalidate();
            }
        }
        /// <summary>
        /// Fonction qui s'occupe de la génération de tuile aléatoire
        /// </summary>
        private void RandomTuile()
        {
            // Tableau pour stocker les indices des cellules vides dans la grille
            List<Tuple<int, int>> caseVide = new List<Tuple<int, int>>();

            // Parcours de la grille pour trouver toutes les cellules vides
            for (int i = 0; i < board.GetLength(0); i++)
            {
                for (int j = 0; j < board.GetLength(1); j++)
                {
                    // Si la cellule est vide (contient la valeur 0)
                    if (board[i, j] == 0) 
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
                var tuile = caseVide[randomIndex];
                int ligne = tuile.Item1;
                int colonne = tuile.Item2;
                // Générer un nombre aléatoire entre 0 et 9 pour décider si la tuile sera 2 ou 4
                // Donne un nombre entre 0 et 9
                int chance = random.Next(10);
                // 10% de chances
                if (chance == 0) 
                {
                    board[ligne, colonne] = 4;
                }
                // 90% de chances
                else
                {
                    board[ligne, colonne] = 2;
                }
            }
        }
        /// <summary>
        /// Fonction principale du jeu. Elle gère les mouvements des tuiles, leurs fusions et l'incréementation du score. 
        /// </summary>
        /// <param name="depHori">Déplacement horizontal</param>
        /// <param name="depVerti">Déplacement vertical</param>
        /// <returns></returns>
        private bool DeplaceTuile(int depHori, int depVerti)
        {
            bool bouge = false;
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
            int[] ordre = new int[4] { 0, 1, 2, 3 };
            if (depHori == 1 || depVerti == 1)
            {
                // Inverser l'ordre pour les mouvements vers la droite ou le bas
                ordre = new int[4] { 3, 2, 1, 0 }; 
            }
            // Parcourir la grille
            for (int x = 0; x < board.GetLength(0); x++)
            {
                for (int y = 0; y < board.GetLength(1); y++)
                {
                    int i = ordre[x];
                    int j = ordre[y];
                    // Ignorer les cases vides
                    if (board[i, j] == 0) continue; 

                    // Trouver la prochaine position
                    int nextI = i + depVerti;
                    int nextJ = j + depHori;

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
                                bouge = true;
                                mouvementTuile.Add(new Tuple<Point, Point>(new Point(i, j), new Point(nextI, nextJ)));
                                break;
                            }
                            else
                            {
                                // Déplacer la tuile sans fusionner
                                if (nextI - depVerti != i || nextJ - depHori != j)
                                {
                                    board[nextI - depVerti, nextJ - depHori] = board[i, j];
                                    if (nextI - depVerti != i || nextJ - depHori != j) board[i, j] = 0;
                                    bouge = true;
                                    mouvementTuile.Add(new Tuple<Point, Point>(new Point(i, j), new Point(nextI - depVerti, nextJ - depHori)));
                                }
                                break;
                            }
                        }
                        nextI += depVerti;
                        nextJ += depHori;
                    }
                    // Gérer le cas où la tuile se déplace vers une case vide en fin de grille
                    if (nextI < 0 || nextI >= 4 || nextJ < 0 || nextJ >= 4)
                    {
                        nextI -= depVerti;
                        nextJ -= depHori;
                        if (nextI != i || nextJ != j)
                        {
                            board[nextI, nextJ] = board[i, j];
                            board[i, j] = 0;
                            bouge = true;
                            mouvementTuile.Add(new Tuple<Point, Point>(new Point(i, j), new Point(nextI, nextJ)));
                        }
                    }
                }
            }
            if (bouge)
            {
                // Met à jour la grille, le score et démarre l'animation si des tuiles ont bougé.
                panelJeu.Invalidate();  
                MajScore(); 
                DebutAnimation(); 
            }
            return bouge;
        }
        /// <summary>
        /// Permet de mettre à jour l'affichage du score
        /// </summary>
        private void MajScore()
        {
            lblScore.Text = $"2048 - Score: {score}";
        }

        /****************************************************************************************************************************************
                    Fonctions utilisées pour le dessin de la grille de jeu dans le Panel de notre Windows Forms. 
        ****************************************************************************************************************************************/

        /// <summary>
        /// DessineGrille dessine de la grille du jeu.
        /// </summary>
        /// <param name="g"></param>
        private void DessineGrille(Graphics g)
        {
            int cellSize = 100;
            Font font = new Font("Arial", 20);

            // Dessinez d'abord toutes les tuiles statiques
            for (int i = 0; i < board.GetLength(0); i++)
            {
                for (int j = 0; j < board.GetLength(1); j++)
                {
                    // Dessinez la tuile si elle n'est pas en cours d'animation
                    if (!tuileAnimations.Any(a => a.Key.X == i && a.Key.Y == j))
                    {
                        DessineTuile(g, new Point(i, j), board[i, j], cellSize, font);
                    }
                }
            }
            // Ensuite, dessinez les tuiles en animation par-dessus
            foreach (var anim in tuileAnimations.Values)
            {
                DessineTuile(g, anim.Actuel, anim.Valeur, cellSize, font);
            }
        }
        /// <summary>
        /// Dessine les tuiles dans la grille.
        /// </summary>
        /// <param name="g"></param>
        /// <param name="gridPosition"></param>
        /// <param name="value"></param>
        /// <param name="cellSize"></param>
        /// <param name="font"></param>
        private void DessineTuile(Graphics g, Point gridPosition, int value, int cellSize, Font font)
        {
            Point screenPosition = ConvertCoordonneeEcran(gridPosition.X, gridPosition.Y, cellSize);
            Color color = ColorTuile(value);
            Brush brush = new SolidBrush(color);
            // Créez un rectangle pour la tuile
            Rectangle rect = new Rectangle(screenPosition.X, screenPosition.Y, cellSize, cellSize);
            // Rayon des coins arrondis
            int cornerRadius = 10;
            // Utiliser GraphicsPath pour créer une forme avec des coins arrondis
            System.Drawing.Drawing2D.GraphicsPath path = new System.Drawing.Drawing2D.GraphicsPath();
            path.AddArc(rect.X, rect.Y, cornerRadius, cornerRadius, 180, 90);
            path.AddArc(rect.X + rect.Width - cornerRadius, rect.Y, cornerRadius, cornerRadius, 270, 90);
            path.AddArc(rect.X + rect.Width - cornerRadius, rect.Y + rect.Height - cornerRadius, cornerRadius, cornerRadius, 0, 90);
            path.AddArc(rect.X, rect.Y + rect.Height - cornerRadius, cornerRadius, cornerRadius, 90, 90);
            path.CloseFigure();
            // Dessinez la tuile avec des coins arrondis
            g.FillPath(brush, path);
            g.DrawPath(Pens.Black, path);


            // Dessinez le texte au centre de la tuile
            string text = value > 0 ? value.ToString() : "";
            g.DrawString(text, font, Brushes.Black, rect, new StringFormat
            {
                Alignment = StringAlignment.Center,
                LineAlignment = StringAlignment.Center
            });
        }
        /// <summary>
        /// Intégre la fonction de dession de la grille dans le panel.
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void panelJeu_Paint(object sender, PaintEventArgs e)
        {
            DessineGrille(e.Graphics);
        }
        /// <summary>
        /// Assure la gestions des couleurs en fonction de la valeur de la tuile.
        /// </summary>
        /// <param name="value">valeur de affichée sur la tuile</param>
        /// <returns></returns>
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

        /****************************************************************************************************************************************
                     Fonctions pour l'animation de la grille de jeu et les calculs de positions de tuiles utilisés pour l'anmiation. 
        ****************************************************************************************************************************************/
        /// <summary>
        /// Initialise l'animation des tuiles à la suite d'un mouvement. 
        /// </summary>
        private void DebutAnimation()
        {
            tuileAnimations.Clear();
            foreach (var movement in mouvementTuile)
            {
                var debut = movement.Item1;
                var fin = movement.Item2;
                int valeur = board[fin.X, fin.Y]; 

                tuileAnimations.Add(debut, new TuileAnimation
                {
                    Debut = debut,
                    Fin = fin,
                    Actuel = debut,
                    Progres = 0.0f,    
                    Valeur= valeur   
                });
            }

            if (tuileAnimations.Any())
                animationTimer.Start();
        }
        /// <summary>
        /// Actionne le calcul de la position des Tuile grâce à CoordoneePosCalc et redessine le tableau dans le Panel. 
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void AnimationTick(object sender, EventArgs e)
        {
            bool TouteTuileDepFin = true;

            foreach (var anim in tuileAnimations)
            {
                if (anim.Value.Progres < 1.0f)
                {
                    anim.Value.Progres += 0.30f; // Ajustez ce taux pour modifier la vitesse de l'animation
                    anim.Value.Actuel = CoordoneePosCalc(anim.Value.Debut, anim.Value.Fin, anim.Value.Progres);
                    TouteTuileDepFin = false;
                }
                else
                {
                    // Mettez à jour le tableau une fois l'animation terminée pour cette tuile
                    board[anim.Value.Fin.X, anim.Value.Fin.Y] = anim.Value.Valeur;
                    board[anim.Value.Debut.X, anim.Value.Debut.Y] = 0;
                }
            }

            panelJeu.Invalidate();

            if (TouteTuileDepFin)
            {
                animationTimer.Stop();
                tuileAnimations.Clear();
                RandomTuile();
                PartieFini();
                PartieGagne();
            }
        }
        /// <summary>
        /// Permet de calculer la position pour les tuiles en mouvement.
        /// </summary>
        /// <param name="debut"> valeur de début</param>
        /// <param name="fin"> valeur de fin</param>
        /// <param name="progres">valeur durant le déplacement.</param>
        /// <returns></returns>
        private int PosCalc(int debut, int fin, float progres)
        {
            return (int)(debut + (fin - debut) * progres);
        }
        /// <summary>
        /// Se base sur PosCalc pour fournir un Point interpolé de X et Y durant le mouvement. 
        /// </summary>
        /// <param name="debut"></param>
        /// <param name="fin"></param>
        /// <param name="progres"></param>
        /// <returns></returns>
        private Point CoordoneePosCalc(Point debut, Point fin, float progres)
        {
            int x = PosCalc(debut.X, fin.X, progres);
            int y = PosCalc(debut.Y, fin.Y, progres);
            return new Point(x, y);
        }
        /// <summary>
        /// Traduit les positions des tuiles en coordonnées de l'écran.
        /// </summary>
        /// <param name="ligne">ligne</param>
        /// <param name="colonne">colonne</param>
        /// <param name="TuileTaille">taille de tuile</param>
        /// <returns></returns>
        private Point ConvertCoordonneeEcran(int ligne, int colonne, int TuileTaille)
        {
            return new Point(colonne * TuileTaille, ligne * TuileTaille);
        }

        /****************************************************************************************************************************************
                                                Boutons à coté de pannel 
        ****************************************************************************************************************************************/

        /// <summary>
        /// Bouton qui ouvre une message box expliquant à l'utilisateur comment jouer.
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void btnHelp_Click(object sender, EventArgs e)
        {
            MessageBox.Show("COMMENT JOUER : Utilisez les flèches du clavier pour déplacer les tuiles. Quand deux tuiles avec le même nombre se touchent, elles fusionnent !");
        }
        /// <summary>
        /// Bouton qui permet de recommencer volontairement la partie.
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void btnRestart_Click(object sender, EventArgs e)
        {
            var recommencer = MessageBox.Show("Voulez-vous recommencer une nouvelle partie?",
                                             "Nouvelle Partie",
                                             MessageBoxButtons.YesNo,
                                             MessageBoxIcon.Question);
            if (recommencer == DialogResult.Yes)
            {
                // La partie est réinitialisée, donc pas vraiment "finie"
                aCliqueContinuer = false;
                InitialisePartie();
            }
        }
    }
}