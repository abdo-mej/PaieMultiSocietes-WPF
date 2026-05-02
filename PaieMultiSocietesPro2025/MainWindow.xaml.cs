using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Media;

namespace PaieMultiSocietesPro2025;

public partial class MainWindow : Window
{
    readonly List<Societe> Societes = new();
    readonly List<User> Users = new();
    readonly List<Salarie> Salaries = new();
    readonly List<Contrat> Contrats = new();
    readonly List<Famille> Familles = new();
    readonly List<Absence> Absences = new();
    readonly List<Rubrique> Rubriques = new();
    readonly List<Bulletin> Bulletins = new();
    readonly List<AuditLog> Audits = new();
    readonly List<PermissionRow> PermissionRows = new();

    User CurrentUser = new();
    Societe CurrentSociete = new();
    string CurrentModule = "Tableau de bord";
    bool DarkMode = false;

    Grid AppGrid = new();
    StackPanel Sidebar = new();
    StackPanel MainPanel = new();
    ComboBox CompanyCombo = new();

    readonly Brush Navy = new SolidColorBrush(Color.FromRgb(11, 27, 48));
    readonly Brush Blue = new SolidColorBrush(Color.FromRgb(37, 99, 235));
    readonly Brush Green = new SolidColorBrush(Color.FromRgb(22, 163, 74));
    readonly Brush Red = new SolidColorBrush(Color.FromRgb(220, 38, 38));
    readonly Brush Orange = new SolidColorBrush(Color.FromRgb(234, 88, 12));
    readonly Brush Purple = new SolidColorBrush(Color.FromRgb(124, 58, 237));
    readonly Brush Slate = new SolidColorBrush(Color.FromRgb(71, 85, 105));

    public MainWindow()
    {
        InitializeComponent();
        SeedData();
        ShowLogin();
    }

    Brush PageBg() => DarkMode ? new SolidColorBrush(Color.FromRgb(15, 23, 42)) : new SolidColorBrush(Color.FromRgb(245, 247, 251));
    Brush CardBg() => DarkMode ? new SolidColorBrush(Color.FromRgb(30, 41, 59)) : Brushes.White;
    Brush TextFg() => DarkMode ? Brushes.White : new SolidColorBrush(Color.FromRgb(15, 23, 42));
    Brush SubFg() => DarkMode ? new SolidColorBrush(Color.FromRgb(203, 213, 225)) : new SolidColorBrush(Color.FromRgb(100, 116, 139));
    Brush BorderFg() => DarkMode ? new SolidColorBrush(Color.FromRgb(51, 65, 85)) : new SolidColorBrush(Color.FromRgb(226, 232, 240));

    void ShowLogin()
    {
        Root.Children.Clear();
        var grid = new Grid { Background = new LinearGradientBrush(Color.FromRgb(248, 250, 252), Color.FromRgb(226, 232, 240), 90) };
        grid.ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(1, GridUnitType.Star) });
        grid.ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(470) });

        var left = new Border
        {
            Margin = new Thickness(50),
            Padding = new Thickness(50),
            CornerRadius = new CornerRadius(28),
            Background = Navy,
            Child = new StackPanel
            {
                VerticalAlignment = VerticalAlignment.Center,
                Children =
                {
                    TextBlockEx("Gestion de Paie", 46, Brushes.White, true),
                    TextBlockEx("Application multi-sociétés", 22, new SolidColorBrush(Color.FromRgb(191,219,254)), false),
                    TextBlockEx("Salariés • Contrats • Famille • Absences • Bulletins • Livre de paie • Permissions", 16, Brushes.White, false),
                    Space(28),
                    TextBlockEx("Version WPF 2025 avec interface claire, rôles utilisateurs et documents imprimables.", 16, new SolidColorBrush(Color.FromRgb(226,232,240)), false)
                }
            }
        };
        Grid.SetColumn(left, 0);
        grid.Children.Add(left);

        var username = new TextBox { Height = 42, Text = "admin", Margin = new Thickness(0, 6, 0, 14), Padding = new Thickness(10) };
        var password = new PasswordBox { Height = 42, Password = "admin123", Margin = new Thickness(0, 6, 0, 20), Padding = new Thickness(10) };
        var card = new Border
        {
            Margin = new Thickness(34),
            Padding = new Thickness(34),
            CornerRadius = new CornerRadius(24),
            Background = Brushes.White,
            Child = new StackPanel
            {
                VerticalAlignment = VerticalAlignment.Center,
                Children =
                {
                    TextBlockEx("Connexion", 32, Brushes.Black, true),
                    TextBlockEx("Nom d'utilisateur", 14, Slate, false), username,
                    TextBlockEx("Mot de passe", 14, Slate, false), password,
                    ActionButton("Se connecter", Blue, () => Login(username.Text, password.Password), 230),
                    Space(14),
                    TextBlockEx("Comptes : admin/admin123, paie/paie123, rh/rh123, lecteur/lecteur123", 12, Slate, false)
                }
            }
        };
        Grid.SetColumn(card, 1);
        grid.Children.Add(card);
        Root.Children.Add(grid);
    }

    void Login(string username, string password)
    {
        var u = Users.FirstOrDefault(x => x.Username.Equals(username, StringComparison.OrdinalIgnoreCase) && x.Password == password && x.Active);
        if (u == null)
        {
            MessageBox.Show("Identifiants incorrects ou compte inactif.", "Connexion", MessageBoxButton.OK, MessageBoxImage.Warning);
            return;
        }
        CurrentUser = u;
        CurrentSociete = Societes.First(s => CurrentUser.SocieteIds.Contains(s.Id));
        Audits.Add(new AuditLog(DateTime.Now, CurrentUser.Username, "Connexion"));
        BuildApp();
    }

    void BuildApp()
    {
        Root.Children.Clear();
        AppGrid = new Grid { Background = PageBg() };
        AppGrid.ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(220) });
        AppGrid.ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(1, GridUnitType.Star) });

        var sideRoot = new Grid { Background = Navy };
        sideRoot.RowDefinitions.Add(new RowDefinition { Height = new GridLength(1, GridUnitType.Star) });
        sideRoot.RowDefinitions.Add(new RowDefinition { Height = GridLength.Auto });
        var scroll = new ScrollViewer { VerticalScrollBarVisibility = ScrollBarVisibility.Auto };
        Sidebar = new StackPanel { Margin = new Thickness(16, 20, 16, 16) };
        Sidebar.Children.Add(TextBlockEx("Gestion Paie", 28, Brushes.White, true));
        Sidebar.Children.Add(TextBlockEx(CurrentUser.FullName, 14, new SolidColorBrush(Color.FromRgb(191,219,254)), false));
        Sidebar.Children.Add(TextBlockEx(CurrentUser.Role, 13, new SolidColorBrush(Color.FromRgb(191,219,254)), false));
        Sidebar.Children.Add(Space(24));
        foreach (var m in MenuFor(CurrentUser)) Sidebar.Children.Add(NavButton(m));
        scroll.Content = Sidebar;
        sideRoot.Children.Add(scroll);
        var logout = ActionButton("Déconnexion", new SolidColorBrush(Color.FromRgb(51,65,85)), ShowLogin, 180);
        logout.Margin = new Thickness(16, 8, 16, 18);
        Grid.SetRow(logout, 1);
        sideRoot.Children.Add(logout);
        Grid.SetColumn(sideRoot, 0);
        AppGrid.Children.Add(sideRoot);

        MainPanel = new StackPanel { Margin = new Thickness(30) };
        var contentScroll = new ScrollViewer { VerticalScrollBarVisibility = ScrollBarVisibility.Auto, Content = MainPanel };
        Grid.SetColumn(contentScroll, 1);
        AppGrid.Children.Add(contentScroll);
        Root.Children.Add(AppGrid);
        ShowModule(CurrentModule);
    }

    IEnumerable<string> MenuFor(User u)
    {
        if (u.Role == "Administrateur") return new[] { "Tableau de bord", "Sociétés", "Utilisateurs", "Permissions", "Salariés", "Contrats", "Famille", "Absences", "Rubriques", "Bulletins", "Livre de paie", "Paramètres", "Audit" };
        if (u.Role == "Responsable Paie") return new[] { "Tableau de bord", "Salariés", "Contrats", "Famille", "Absences", "Rubriques", "Bulletins", "Livre de paie", "Paramètres" };
        if (u.Role == "Gestionnaire RH") return new[] { "Tableau de bord", "Salariés", "Contrats", "Famille", "Absences" };
        return new[] { "Tableau de bord", "Salariés", "Bulletins", "Livre de paie" };
    }

    Button NavButton(string text)
    {
        var selected = text == CurrentModule;
        return ActionButton(text, selected ? Blue : Brushes.Transparent, () => ShowModule(text), 180, Brushes.White);
    }

    void ShowModule(string module)
    {
        CurrentModule = module;
        if (MainPanel == null) return;
        MainPanel.Children.Clear();
        Header(module);
        switch (module)
        {
            case "Tableau de bord": DashboardPage(); break;
            case "Sociétés": SocietesPage(); break;
            case "Utilisateurs": UsersPage(); break;
            case "Permissions": PermissionsPage(); break;
            case "Salariés": SalariesPage(); break;
            case "Contrats": ContratsPage(); break;
            case "Famille": FamillePage(); break;
            case "Absences": AbsencesPage(); break;
            case "Rubriques": RubriquesPage(); break;
            case "Bulletins": BulletinsPage(); break;
            case "Livre de paie": LivrePage(); break;
            case "Paramètres": ParamPage(); break;
            case "Audit": AuditPage(); break;
        }
    }

    void Header(string title)
    {
        var dock = new DockPanel { Margin = new Thickness(0, 0, 0, 22) };
        var right = new StackPanel { Orientation = Orientation.Horizontal, HorizontalAlignment = HorizontalAlignment.Right };
        right.Children.Add(ActionButton(DarkMode ? "Mode clair" : "Mode sombre", Slate, () => { DarkMode = !DarkMode; BuildApp(); }, 125));
        right.Children.Add(Space(10));
        CompanyCombo = new ComboBox { Width = 300, Height = 40, Margin = new Thickness(8,0,8,0), ItemsSource = Societes.Where(s => CurrentUser.SocieteIds.Contains(s.Id)).ToList(), DisplayMemberPath = "RaisonSociale", SelectedItem = CurrentSociete };
        CompanyCombo.SelectionChanged += (_, _) => { if (CompanyCombo.SelectedItem is Societe s) { CurrentSociete = s; ShowModule(CurrentModule); } };
        right.Children.Add(CompanyCombo);
        right.Children.Add(Space(10));
        right.Children.Add(ActionButton("Déconnexion", Red, ShowLogin, 115));
        DockPanel.SetDock(right, Dock.Right);
        dock.Children.Add(right);
        dock.Children.Add(new StackPanel { Children = { TextBlockEx(title, 34, TextFg(), true), TextBlockEx($"Périmètre : {CurrentSociete.RaisonSociale} | Utilisateur : {CurrentUser.FullName} | Rôle : {CurrentUser.Role}", 14, SubFg(), false) } });
        MainPanel.Children.Add(dock);
    }

    void DashboardPage()
    {
        EnsureBulletins();
        MainPanel.Children.Add(Cards(new[] {
            ("Sociétés accessibles", CurrentUser.SocieteIds.Count.ToString()),
            ("Salariés actifs", Salaries.Count(s => s.SocieteId == CurrentSociete.Id && s.Statut == "Actif").ToString()),
            ("Contrats actifs", Contrats.Count(c => Emp(c.SalarieId).SocieteId == CurrentSociete.Id && c.Statut == "Actif").ToString()),
            ("Net à payer", Bulletins.Where(b => b.SocieteId == CurrentSociete.Id).Sum(b => b.NetAPayer).ToString("N2") + " MAD")
        }));
        MainPanel.Children.Add(Section("Derniers bulletins générés", DataTable(Bulletins.Where(b => b.SocieteId == CurrentSociete.Id).OrderByDescending(b => b.DateGeneration).Select(BulletinRow).ToList(), "dgDash")));
    }

    void SocietesPage()
    {
        Toolbar(("Ajouter", Green, () => OpenSociete(null)), ("Modifier", Blue, () => EditSelected<Societe>("dgSoc", OpenSociete)), ("Supprimer", Red, () => DeleteSelected("dgSoc", Societes)));
        MainPanel.Children.Add(Section("Liste des sociétés", DataTable(Societes.ToList(), "dgSoc")));
    }

    void UsersPage()
    {
        Toolbar(("Ajouter", Green, () => OpenUser(null)), ("Modifier", Blue, () => EditSelected<User>("dgUsers", OpenUser)), ("Activer/Désactiver", Orange, ToggleUser));
        MainPanel.Children.Add(Section("Comptes utilisateurs", DataTable(Users.ToList(), "dgUsers")));
    }

    void PermissionsPage()
    {
        Toolbar(("Enregistrer", Green, () => MessageBox.Show("Permissions enregistrées.")), ("Tout cocher", Blue, () => { foreach (var p in PermissionRows) p.SetAll(true); Refresh(); }), ("Réinitialiser", Orange, ResetPermissions));
        MainPanel.Children.Add(Section("Matrice des permissions par rôle et module", DataTable(PermissionRows, "dgPerm", false)));
    }

    void SalariesPage()
    {
        Toolbar(("Ajouter", Green, () => OpenSalarie(null)), ("Modifier", Blue, () => EditSelected<Salarie>("dgSal", OpenSalarie)), ("Supprimer", Red, () => DeleteSelected("dgSal", Salaries)));
        MainPanel.Children.Add(Section("Salariés de la société", DataTable(Salaries.Where(s => s.SocieteId == CurrentSociete.Id).ToList(), "dgSal")));
    }

    void ContratsPage()
    {
        Toolbar(("Nouveau contrat", Green, () => OpenContrat(null)), ("Modifier", Blue, () => EditSelected<Contrat>("dgCtr", OpenContrat)), ("Aperçu", Purple, () => PreviewDocument(ContratDocument())), ("Imprimer / PDF", Orange, () => PrintDocument(ContratDocument())));
        MainPanel.Children.Add(Section("Contrats de travail", DataTable(Contrats.Where(c => Emp(c.SalarieId).SocieteId == CurrentSociete.Id).ToList(), "dgCtr")));
    }

    void FamillePage()
    {
        Toolbar(("Ajouter", Green, () => OpenFamille(null)), ("Modifier", Blue, () => EditSelected<Famille>("dgFam", OpenFamille)), ("Supprimer", Red, () => DeleteSelected("dgFam", Familles)));
        MainPanel.Children.Add(Section("Ayants droit / Famille", DataTable(Familles.Where(f => Emp(f.SalarieId).SocieteId == CurrentSociete.Id).ToList(), "dgFam")));
    }

    void AbsencesPage()
    {
        Toolbar(("Saisir absence", Green, () => OpenAbsence(null)), ("Modifier", Blue, () => EditSelected<Absence>("dgAbs", OpenAbsence)), ("Valider", Green, ValidateAbsence));
        MainPanel.Children.Add(Section("Absences et congés", DataTable(Absences.Where(a => Emp(a.SalarieId).SocieteId == CurrentSociete.Id).ToList(), "dgAbs")));
    }

    void RubriquesPage()
    {
        Toolbar(("Ajouter", Green, () => OpenRubrique(null)), ("Modifier", Blue, () => EditSelected<Rubrique>("dgRub", OpenRubrique)), ("Supprimer", Red, () => DeleteSelected("dgRub", Rubriques)));
        MainPanel.Children.Add(Section("Rubriques de paie", DataTable(Rubriques.ToList(), "dgRub")));
    }

    void BulletinsPage()
    {
        Toolbar(("Générer", Green, () => { GenerateBulletins(true); ShowModule(CurrentModule); }), ("Aperçu", Purple, () => PreviewDocument(BulletinDocument())), ("Imprimer / PDF", Orange, () => PrintDocument(BulletinDocument())));
        EnsureBulletins();
        MainPanel.Children.Add(Section("Bulletins de paie", DataTable(Bulletins.Where(b => b.SocieteId == CurrentSociete.Id).Select(BulletinRow).ToList(), "dgBul")));
    }

    void LivrePage()
    {
        Toolbar(("Aperçu livre", Purple, () => PreviewDocument(LivreDocument())), ("Imprimer / PDF", Orange, () => PrintDocument(LivreDocument())));
        EnsureBulletins();
        var rows = Bulletins.Where(b => b.SocieteId == CurrentSociete.Id).Select(BulletinRow).ToList();
        MainPanel.Children.Add(Section("Livre de paie annuel", DataTable(rows, "dgLivre")));
    }

    void ParamPage()
    {
        Toolbar(("Modifier société", Blue, () => OpenSociete(CurrentSociete)), (DarkMode ? "Mode clair" : "Mode sombre", Slate, () => { DarkMode = !DarkMode; BuildApp(); }));
        MainPanel.Children.Add(Section("Paramètres de la société sélectionnée", DataTable(new[] { CurrentSociete }, "dgParam")));
        MainPanel.Children.Add(Section("Paramètres de calcul", DataTable(new[] { new { CNSS = CurrentSociete.TauxCnss + " %", AMO = CurrentSociete.TauxAmo + " %", IR = CurrentSociete.TauxIr + " %", Devise = "MAD", Periode = "05/2026" } }.ToList(), "dgTaux")));
    }

    void AuditPage()
    {
        MainPanel.Children.Add(Section("Journal d'audit", DataTable(Audits.OrderByDescending(a => a.Date).ToList(), "dgAudit")));
    }

    void Toolbar(params (string text, Brush color, Action action)[] actions)
    {
        var panel = new StackPanel { Orientation = Orientation.Horizontal, Margin = new Thickness(0, 0, 0, 18) };
        foreach (var a in actions) panel.Children.Add(ActionButton(a.text, a.color, a.action, 150));
        MainPanel.Children.Add(panel);
    }

    DataGrid DataTable(IEnumerable source, string name, bool readOnly = true)
    {
        var dg = new DataGrid { Name = name, ItemsSource = source, IsReadOnly = readOnly, AutoGenerateColumns = true, MinHeight = 260, MaxHeight = 520, Margin = new Thickness(0, 10, 0, 0), CanUserAddRows = false, Background = CardBg(), Foreground = TextFg(), GridLinesVisibility = DataGridGridLinesVisibility.Horizontal, HeadersVisibility = DataGridHeadersVisibility.Column };
        dg.AutoGeneratingColumn += (_, e) => { if (e.PropertyName.Equals("Password", StringComparison.OrdinalIgnoreCase)) e.Cancel = true; };
        dg.Loaded += (_, _) => { foreach (var c in dg.Columns) c.Width = new DataGridLength(1, DataGridLengthUnitType.Star); };
        return dg;
    }

    T? Selected<T>(string name) where T : class
    {
        var dg = FindChild<DataGrid>(Root, name);
        return dg?.SelectedItem as T;
    }

    void EditSelected<T>(string gridName, Action<T> action) where T : class
    {
        var item = Selected<T>(gridName);
        if (item == null) { MessageBox.Show("Sélectionnez une ligne dans le tableau."); return; }
        action(item);
    }

    void DeleteSelected<T>(string gridName, List<T> list) where T : class
    {
        var item = Selected<T>(gridName);
        if (item == null) { MessageBox.Show("Sélectionnez une ligne à supprimer."); return; }
        if (MessageBox.Show("Confirmer la suppression ?", "Suppression", MessageBoxButton.YesNo, MessageBoxImage.Question) == MessageBoxResult.Yes)
        {
            list.Remove(item);
            Audits.Add(new AuditLog(DateTime.Now, CurrentUser.Username, "Suppression"));
            Refresh();
        }
    }

    void ToggleUser()
    {
        var u = Selected<User>("dgUsers");
        if (u == null) { MessageBox.Show("Sélectionnez un utilisateur."); return; }
        u.Active = !u.Active;
        Refresh();
    }

    void ValidateAbsence()
    {
        var a = Selected<Absence>("dgAbs");
        if (a == null) { MessageBox.Show("Sélectionnez une absence."); return; }
        a.Statut = "Validée";
        Refresh();
    }

    void Refresh() => ShowModule(CurrentModule);

    Window Dialog(string title)
    {
        return new Window { Title = title, Width = 720, Height = 650, MinWidth = 620, MinHeight = 500, WindowStartupLocation = WindowStartupLocation.CenterOwner, Owner = this, Background = PageBg() };
    }

    bool ShowForm(string title, IEnumerable<(string label, Control input)> fields)
    {
        var win = Dialog(title);
        var stack = new StackPanel { Margin = new Thickness(24) };
        stack.Children.Add(TextBlockEx(title, 26, TextFg(), true));
        foreach (var f in fields)
        {
            stack.Children.Add(TextBlockEx(f.label, 14, TextFg(), true));
            f.input.Margin = new Thickness(0, 4, 0, 14);
            f.input.MinHeight = 36;
            stack.Children.Add(f.input);
        }
        var buttons = new StackPanel { Orientation = Orientation.Horizontal, HorizontalAlignment = HorizontalAlignment.Right };
        bool saved = false;
        buttons.Children.Add(ActionButton("Annuler", Slate, () => win.Close(), 110));
        buttons.Children.Add(ActionButton("Enregistrer", Green, () => { saved = true; win.Close(); }, 130));
        stack.Children.Add(buttons);
        win.Content = new ScrollViewer { Content = stack, VerticalScrollBarVisibility = ScrollBarVisibility.Auto };
        win.ShowDialog();
        return saved;
    }

    TextBox Input(string text = "") => new TextBox { Text = text, Padding = new Thickness(8) };
    ComboBox Combo(IEnumerable items, object? selected = null) => new ComboBox { ItemsSource = items, SelectedItem = selected, Padding = new Thickness(8) };

    void OpenSociete(Societe? s)
    {
        var obj = s ?? new Societe { Id = NextId(Societes), TauxCnss = 4.48, TauxAmo = 2.26, TauxIr = 6.52 };
        var raison = Input(obj.RaisonSociale); var code = Input(obj.Code); var forme = Input(obj.Forme); var ville = Input(obj.Ville); var adresse = Input(obj.Adresse); var ice = Input(obj.ICE); var cnss = Input(obj.CNSS);
        if (ShowForm(s == null ? "Ajouter une société" : "Modifier société", new[] { ("Raison sociale", (Control)raison), ("Code", code), ("Forme juridique", forme), ("Ville", ville), ("Adresse", adresse), ("ICE", ice), ("CNSS", cnss) }))
        {
            obj.RaisonSociale = raison.Text; obj.Code = code.Text; obj.Forme = forme.Text; obj.Ville = ville.Text; obj.Adresse = adresse.Text; obj.ICE = ice.Text; obj.CNSS = cnss.Text;
            if (s == null) Societes.Add(obj); Refresh();
        }
    }

    void OpenUser(User? u)
    {
        var obj = u ?? new User { Id = NextId(Users), Active = true, SocieteIds = new List<int> { CurrentSociete.Id } };
        var username = Input(obj.Username); var full = Input(obj.FullName); var pass = Input(obj.Password); var role = Combo(new[] { "Administrateur", "Responsable Paie", "Gestionnaire RH", "Lecteur" }, obj.Role); var active = new CheckBox { IsChecked = obj.Active, Content = "Compte actif", Foreground = TextFg() };
        if (ShowForm(u == null ? "Ajouter un utilisateur" : "Modifier utilisateur", new[] { ("Nom d'utilisateur", (Control)username), ("Nom complet", full), ("Mot de passe", pass), ("Rôle", role), ("Statut", active) }))
        {
            obj.Username = username.Text; obj.FullName = full.Text; obj.Password = pass.Text; obj.Role = role.SelectedItem?.ToString() ?? "Lecteur"; obj.Active = active.IsChecked == true;
            if (u == null) Users.Add(obj); Refresh();
        }
    }

    void OpenSalarie(Salarie? s)
    {
        var obj = s ?? new Salarie { Id = NextId(Salaries), SocieteId = CurrentSociete.Id, Statut = "Actif", DateEntree = DateTime.Today };
        var matricule = Input(obj.Matricule); var nom = Input(obj.Nom); var prenom = Input(obj.Prenom); var cin = Input(obj.CIN); var fonction = Input(obj.Fonction); var salaire = Input(obj.SalaireBase.ToString()); var primes = Input(obj.Primes.ToString()); var statut = Combo(new[] { "Actif", "Suspendu", "Sorti" }, obj.Statut);
        if (ShowForm(s == null ? "Ajouter salarié" : "Modifier salarié", new[] { ("Matricule", (Control)matricule), ("Nom", nom), ("Prénom", prenom), ("CIN", cin), ("Fonction", fonction), ("Salaire de base", salaire), ("Primes", primes), ("Statut", statut) }))
        {
            obj.Matricule = matricule.Text; obj.Nom = nom.Text; obj.Prenom = prenom.Text; obj.CIN = cin.Text; obj.Fonction = fonction.Text; obj.SalaireBase = Parse(salaire.Text); obj.Primes = Parse(primes.Text); obj.Statut = statut.SelectedItem?.ToString() ?? "Actif";
            if (s == null) Salaries.Add(obj); Refresh();
        }
    }

    void OpenContrat(Contrat? c)
    {
        var emps = Salaries.Where(x => x.SocieteId == CurrentSociete.Id).ToList();
        var obj = c ?? new Contrat { Id = NextId(Contrats), SalarieId = emps.FirstOrDefault()?.Id ?? 0, TypeContrat = "CDI", Statut = "Actif", DateDebut = DateTime.Today };
        var sal = Combo(emps, emps.FirstOrDefault(x => x.Id == obj.SalarieId)); sal.DisplayMemberPath = "NomComplet";
        var type = Combo(new[] { "CDI", "CDD", "Stage", "Temps partiel" }, obj.TypeContrat); var poste = Input(obj.Poste); var salaire = Input(obj.SalaireBase.ToString()); var statut = Combo(new[] { "Actif", "Terminé", "Suspendu" }, obj.Statut);
        if (ShowForm(c == null ? "Nouveau contrat" : "Modifier contrat", new[] { ("Salarié", (Control)sal), ("Type de contrat", type), ("Poste", poste), ("Salaire contractuel", salaire), ("Statut", statut) }))
        {
            if (sal.SelectedItem is Salarie selected) obj.SalarieId = selected.Id; obj.TypeContrat = type.SelectedItem?.ToString() ?? "CDI"; obj.Poste = poste.Text; obj.SalaireBase = Parse(salaire.Text); obj.Statut = statut.SelectedItem?.ToString() ?? "Actif";
            if (c == null) Contrats.Add(obj); Refresh();
        }
    }

    void OpenFamille(Famille? f)
    {
        var emps = Salaries.Where(x => x.SocieteId == CurrentSociete.Id).ToList(); var obj = f ?? new Famille { Id = NextId(Familles), SalarieId = emps.FirstOrDefault()?.Id ?? 0, DateNaissance = DateTime.Today.AddYears(-5) };
        var sal = Combo(emps, emps.FirstOrDefault(x => x.Id == obj.SalarieId)); sal.DisplayMemberPath = "NomComplet"; var nom = Input(obj.NomComplet); var lien = Combo(new[] { "Conjoint", "Enfant", "Parent" }, obj.Lien);
        if (ShowForm(f == null ? "Ajouter ayant droit" : "Modifier ayant droit", new[] { ("Salarié", (Control)sal), ("Nom complet", nom), ("Lien", lien) }))
        { if (sal.SelectedItem is Salarie selected) obj.SalarieId = selected.Id; obj.NomComplet = nom.Text; obj.Lien = lien.SelectedItem?.ToString() ?? "Enfant"; if (f == null) Familles.Add(obj); Refresh(); }
    }

    void OpenAbsence(Absence? a)
    {
        var emps = Salaries.Where(x => x.SocieteId == CurrentSociete.Id).ToList(); var obj = a ?? new Absence { Id = NextId(Absences), SalarieId = emps.FirstOrDefault()?.Id ?? 0, Type = "Congé payé", DateDebut = DateTime.Today, DateFin = DateTime.Today, Jours = 1, Statut = "En attente" };
        var sal = Combo(emps, emps.FirstOrDefault(x => x.Id == obj.SalarieId)); sal.DisplayMemberPath = "NomComplet"; var type = Combo(new[] { "Congé payé", "Maladie", "Absence non justifiée", "Autorisation" }, obj.Type); var jours = Input(obj.Jours.ToString()); var statut = Combo(new[] { "En attente", "Validée", "Refusée" }, obj.Statut);
        if (ShowForm(a == null ? "Saisir absence" : "Modifier absence", new[] { ("Salarié", (Control)sal), ("Type", type), ("Nombre de jours", jours), ("Statut", statut) }))
        { if (sal.SelectedItem is Salarie selected) obj.SalarieId = selected.Id; obj.Type = type.SelectedItem?.ToString() ?? "Congé payé"; obj.Jours = Parse(jours.Text); obj.Statut = statut.SelectedItem?.ToString() ?? "En attente"; if (a == null) Absences.Add(obj); Refresh(); }
    }

    void OpenRubrique(Rubrique? r)
    {
        var obj = r ?? new Rubrique { Id = NextId(Rubriques), Type = "Gain" }; var code = Input(obj.Code); var lib = Input(obj.Libelle); var type = Combo(new[] { "Gain", "Retenue", "Patronale" }, obj.Type); var taux = Input(obj.Taux.ToString());
        if (ShowForm(r == null ? "Ajouter rubrique" : "Modifier rubrique", new[] { ("Code", (Control)code), ("Libellé", lib), ("Type", type), ("Taux", taux) }))
        { obj.Code = code.Text; obj.Libelle = lib.Text; obj.Type = type.SelectedItem?.ToString() ?? "Gain"; obj.Taux = Parse(taux.Text); if (r == null) Rubriques.Add(obj); Refresh(); }
    }

    void GenerateBulletins(bool replace)
    {
        string periode = "05/2026";
        if (replace) Bulletins.RemoveAll(b => b.SocieteId == CurrentSociete.Id && b.Periode == periode);
        foreach (var s in Salaries.Where(x => x.SocieteId == CurrentSociete.Id && x.Statut == "Actif"))
        {
            if (replace || !Bulletins.Any(b => b.SalarieId == s.Id && b.Periode == periode))
            {
                var brut = s.SalaireBase + s.Primes;
                var cnss = brut * CurrentSociete.TauxCnss / 100;
                var amo = brut * CurrentSociete.TauxAmo / 100;
                var ir = brut * CurrentSociete.TauxIr / 100;
                var retenues = cnss + amo + ir;
                var net = brut - retenues;
                var patronal = brut * 0.1698;
                Bulletins.Add(new Bulletin { Id = NextId(Bulletins), SocieteId = CurrentSociete.Id, SalarieId = s.Id, Periode = periode, Brut = brut, Cnss = cnss, Amo = amo, Ir = ir, Retenues = retenues, NetAPayer = net, CotisationsPatronales = patronal, CoutGlobal = brut + patronal, DateGeneration = DateTime.Now });
            }
        }
        Audits.Add(new AuditLog(DateTime.Now, CurrentUser.Username, "Génération bulletins " + periode));
    }

    void EnsureBulletins() { if (!Bulletins.Any(b => b.SocieteId == CurrentSociete.Id)) GenerateBulletins(false); }

    FlowDocument BulletinDocument()
    {
        EnsureBulletins();
        var b = Bulletins.FirstOrDefault(x => x.SocieteId == CurrentSociete.Id) ?? Bulletins.First();
        var s = Emp(b.SalarieId);
        var doc = BaseDoc(false);
        var head = DocTable(new[] { "Employeur", "Bulletin" });
        AddDocRow(head, CurrentSociete.RaisonSociale + "\n" + CurrentSociete.Adresse + "\nICE : " + CurrentSociete.ICE + "\nCNSS : " + CurrentSociete.CNSS, "BULLETIN DE PAIE\nPériode : " + b.Periode + "\nPaiement : " + DateTime.Today.ToString("dd/MM/yyyy") + "\nMode : Virement", true);
        doc.Blocks.Add(head);
        doc.Blocks.Add(new Paragraph(new Run($"Salarié : {s.NomComplet}     Matricule : {s.Matricule}     CIN : {s.CIN}\nFonction : {s.Fonction}     Entrée : {s.DateEntree:dd/MM/yyyy}")) { FontWeight = FontWeights.Bold, FontSize = 12 });
        var t = DocTable(new[] { "Rubriques", "Base", "Taux salarial", "Montant salarial", "Taux patronal", "Cot. patronales" });
        AddDocRow(t, "Salaire de base", s.SalaireBase, "", s.SalaireBase, "", "");
        AddDocRow(t, "Primes et indemnités", s.Primes, "", s.Primes, "", "");
        AddDocRow(t, "Salaire brut", b.Brut, "", b.Brut, "", "", true);
        AddDocRow(t, "Cotisation CNSS salariale", b.Brut, CurrentSociete.TauxCnss + " %", b.Cnss, "16.98 %", b.CotisationsPatronales);
        AddDocRow(t, "Cotisation AMO salariale", b.Brut, CurrentSociete.TauxAmo + " %", b.Amo, "", "");
        AddDocRow(t, "Impôt sur revenu", b.Brut, "Barème", b.Ir, "", "");
        AddDocRow(t, "TOTAL RETENUES", "", "", b.Retenues, "", b.CotisationsPatronales, true);
        AddDocRow(t, "NET À PAYER", "", "", b.NetAPayer, "", "", true);
        doc.Blocks.Add(t);
        doc.Blocks.Add(new Paragraph(new Run($"Base imposable : {(b.Brut-b.Retenues):N2} MAD      Total retenues : {b.Retenues:N2} MAD      Coût global période : {b.CoutGlobal:N2} MAD")) { FontSize = 11 });
        doc.Blocks.Add(new Paragraph(new Run($"NET À PAYER : {b.NetAPayer:N2} MAD")) { FontSize = 20, FontWeight = FontWeights.Bold, TextAlignment = TextAlignment.Right });
        return doc;
    }

    FlowDocument ContratDocument()
    {
        var c = Contrats.FirstOrDefault(x => Emp(x.SalarieId).SocieteId == CurrentSociete.Id) ?? Contrats.First();
        var s = Emp(c.SalarieId);
        var doc = BaseDoc(false);
        doc.Blocks.Add(new Paragraph(new Run("CONTRAT DE TRAVAIL")) { FontSize = 26, FontWeight = FontWeights.Bold, TextAlignment = TextAlignment.Center });
        doc.Blocks.Add(new Paragraph(new Run($"Entre les soussignés :\n\nLa société {CurrentSociete.RaisonSociale}, forme juridique {CurrentSociete.Forme}, sise à {CurrentSociete.Adresse}, représentée par son gérant, ci-après dénommée l'Employeur.\n\nEt M./Mme {s.NomComplet}, CIN {s.CIN}, matricule {s.Matricule}, ci-après dénommé le Salarié.\n\nArticle 1 - Nature du contrat : {c.TypeContrat}\nArticle 2 - Poste : {c.Poste}\nArticle 3 - Date de début : {c.DateDebut:dd/MM/yyyy}\nArticle 4 - Rémunération : {c.SalaireBase:N2} MAD brut mensuel\nArticle 5 - Obligations : le salarié s'engage à respecter le règlement intérieur, la confidentialité et les procédures de l'entreprise.\nArticle 6 - Déclarations sociales : l'employeur s'engage à déclarer le salarié aux organismes compétents.\n\nFait à {CurrentSociete.Ville}, le {DateTime.Today:dd/MM/yyyy}\n\nSignature employeur                                      Signature salarié")) { FontSize = 14, LineHeight = 26 });
        return doc;
    }

    FlowDocument LivreDocument()
    {
        EnsureBulletins();
        var doc = BaseDoc(true);
        doc.Blocks.Add(new Paragraph(new Run("LIVRE DE PAIE ANNUEL - " + CurrentSociete.RaisonSociale)) { FontSize = 24, FontWeight = FontWeights.Bold });
        doc.Blocks.Add(new Paragraph(new Run($"Période : Année 2026 | Adresse : {CurrentSociete.Adresse} | ICE : {CurrentSociete.ICE}")) { FontSize = 11 });
        var t = DocTable(new[] { "Salarié", "Base", "Primes", "Brut", "CNSS", "AMO", "IR", "Retenues", "Net" });
        foreach (var b in Bulletins.Where(x => x.SocieteId == CurrentSociete.Id))
        {
            var s = Emp(b.SalarieId);
            AddDocRow(t, s.NomComplet, s.SalaireBase, s.Primes, b.Brut, b.Cnss, b.Amo, b.Ir, b.Retenues, b.NetAPayer);
        }
        AddDocRow(t, "TOTAL", "", "", Bulletins.Where(x => x.SocieteId == CurrentSociete.Id).Sum(x => x.Brut), "", "", "", Bulletins.Where(x => x.SocieteId == CurrentSociete.Id).Sum(x => x.Retenues), Bulletins.Where(x => x.SocieteId == CurrentSociete.Id).Sum(x => x.NetAPayer), true);
        doc.Blocks.Add(t);
        return doc;
    }

    FlowDocument BaseDoc(bool landscape)
    {
        return new FlowDocument { PageWidth = landscape ? 1122 : 793, PageHeight = landscape ? 793 : 1122, PagePadding = new Thickness(40), ColumnWidth = landscape ? 1040 : 700, FontFamily = new FontFamily("Arial") };
    }

    Table DocTable(string[] headers)
    {
        var t = new Table { CellSpacing = 0, BorderBrush = Brushes.Black, BorderThickness = new Thickness(1) };
        foreach (var _ in headers) t.Columns.Add(new TableColumn());
        var rg = new TableRowGroup();
        var row = new TableRow();
        foreach (var h in headers) row.Cells.Add(new TableCell(new Paragraph(new Run(h))) { FontWeight = FontWeights.Bold, Background = new SolidColorBrush(Color.FromRgb(226, 232, 240)), BorderBrush = Brushes.Black, BorderThickness = new Thickness(.5), Padding = new Thickness(5) });
        rg.Rows.Add(row); t.RowGroups.Add(rg); return t;
    }
    void AddDocRow(Table t, params object[] values) => AddDocRow(t, false, values);
    void AddDocRow(Table t, object a, object b, object c, object d, object e, object f, bool bold) => AddDocRow(t, bold, new[] { a, b, c, d, e, f });
    void AddDocRow(Table t, object a, object b, object c, object d, object e, object f, object g, object h, object i, bool bold) => AddDocRow(t, bold, new[] { a, b, c, d, e, f, g, h, i });
    void AddDocRow(Table t, bool bold, params object[] values)
    {
        var row = new TableRow();
        foreach (var v in values)
        {
            var txt = v is double d ? d.ToString("N2") : (v?.ToString() ?? "");
            row.Cells.Add(new TableCell(new Paragraph(new Run(txt))) { FontWeight = bold ? FontWeights.Bold : FontWeights.Normal, BorderBrush = Brushes.Black, BorderThickness = new Thickness(.5), Padding = new Thickness(5) });
        }
        t.RowGroups[0].Rows.Add(row);
    }

    void PrintDocument(FlowDocument doc)
    {
        var dlg = new PrintDialog();
        if (dlg.ShowDialog() == true) dlg.PrintDocument(((IDocumentPaginatorSource)doc).DocumentPaginator, "Document Paie");
    }
    void PreviewDocument(FlowDocument doc)
    {
        new Window { Title = "Aperçu document", Width = 960, Height = 720, Owner = this, WindowStartupLocation = WindowStartupLocation.CenterOwner, Content = new FlowDocumentScrollViewer { Document = doc, VerticalScrollBarVisibility = ScrollBarVisibility.Auto } }.ShowDialog();
    }

    BulletinRow BulletinRow(Bulletin b)
    {
        var s = Emp(b.SalarieId);
        return new BulletinRow { Id = b.Id, Periode = b.Periode, Salarie = s.NomComplet, Brut = b.Brut, Cnss = b.Cnss, Amo = b.Amo, Ir = b.Ir, Retenues = b.Retenues, NetAPayer = b.NetAPayer, DateGeneration = b.DateGeneration };
    }

    Salarie Emp(int id) => Salaries.First(x => x.Id == id);
    int NextId<T>(List<T> list) => list.Count + 1;
    double Parse(string s) => double.TryParse(s.Replace(',', '.'), System.Globalization.NumberStyles.Any, System.Globalization.CultureInfo.InvariantCulture, out var v) ? v : 0;

    Border Cards(IEnumerable<(string label, string value)> data)
    {
        var wrap = new WrapPanel { Margin = new Thickness(0, 0, 0, 16) };
        foreach (var d in data)
        {
            wrap.Children.Add(new Border { Width = 240, Height = 110, Margin = new Thickness(0, 0, 14, 14), Padding = new Thickness(18), Background = CardBg(), BorderBrush = BorderFg(), BorderThickness = new Thickness(1), CornerRadius = new CornerRadius(16), Child = new StackPanel { Children = { TextBlockEx(d.label, 14, SubFg(), false), TextBlockEx(d.value, 28, TextFg(), true) } } });
        }
        return new Border { Child = wrap };
    }

    FrameworkElement Section(string title, UIElement body)
    {
        return new Border { Padding = new Thickness(22), Margin = new Thickness(0, 0, 0, 18), Background = CardBg(), BorderBrush = BorderFg(), BorderThickness = new Thickness(1), CornerRadius = new CornerRadius(18), Child = new StackPanel { Children = { TextBlockEx(title, 24, TextFg(), true), body } } };
    }

    TextBlock TextBlockEx(string text, int size, Brush color, bool bold)
    {
        return new TextBlock { Text = text, FontSize = size, Foreground = color, FontWeight = bold ? FontWeights.Bold : FontWeights.Normal, Margin = new Thickness(0, 4, 0, 6), TextWrapping = TextWrapping.Wrap };
    }
    FrameworkElement Space(double w) => new Border { Width = w, Height = w, Background = Brushes.Transparent };

    Button ActionButton(string text, Brush bg, Action action, double width = 140, Brush? fg = null)
    {
        var b = new Button { Content = text, Width = width, Height = 40, Margin = new Thickness(0, 0, 10, 10), Background = bg, Foreground = fg ?? Brushes.White, BorderThickness = new Thickness(0), FontWeight = FontWeights.SemiBold };
        b.Click += (_, _) => action();
        return b;
    }

    static T? FindChild<T>(DependencyObject parent, string name) where T : FrameworkElement
    {
        for (int i = 0; i < VisualTreeHelper.GetChildrenCount(parent); i++)
        {
            var child = VisualTreeHelper.GetChild(parent, i);
            if (child is T t && (string.IsNullOrEmpty(name) || t.Name == name)) return t;
            var r = FindChild<T>(child, name);
            if (r != null) return r;
        }
        return null;
    }

    IEnumerable<string> MenuForUser(User u) => MenuFor(u);
    void ResetPermissions()
    {
        PermissionRows.Clear();
        foreach (var role in new[] { "Administrateur", "Responsable Paie", "Gestionnaire RH", "Lecteur" })
        foreach (var m in new[] { "Sociétés", "Utilisateurs", "Salariés", "Contrats", "Famille", "Absences", "Rubriques", "Bulletins", "Livre de paie", "Paramètres" })
            PermissionRows.Add(new PermissionRow(role, m));
        Refresh();
    }

    void SeedData()
    {
        Societes.AddRange(new[] {
            new Societe{ Id=1, RaisonSociale="ALPHA SERVICES", Code="ALPHA", Forme="SARL", Ville="Oujda", Adresse="Bd Mohammed V, Oujda", ICE="003005149000022", CNSS="2890741", Principale=true, TauxCnss=4.48, TauxAmo=2.26, TauxIr=6.52 },
            new Societe{ Id=2, RaisonSociale="BETA INDUSTRIES", Code="BETA", Forme="SA", Ville="Casablanca", Adresse="Twin Center, Casablanca", ICE="003005149000033", CNSS="2890742", Principale=false, TauxCnss=4.48, TauxAmo=2.26, TauxIr=6.52 }
        });
        Users.AddRange(new[] {
            new User{Id=1,Username="admin",Password="admin123",FullName="Administrateur Système",Role="Administrateur",Active=true,SocieteIds=new List<int>{1,2}},
            new User{Id=2,Username="paie",Password="paie123",FullName="Responsable Paie",Role="Responsable Paie",Active=true,SocieteIds=new List<int>{1}},
            new User{Id=3,Username="rh",Password="rh123",FullName="Gestionnaire RH",Role="Gestionnaire RH",Active=true,SocieteIds=new List<int>{1}},
            new User{Id=4,Username="lecteur",Password="lecteur123",FullName="Lecteur",Role="Lecteur",Active=true,SocieteIds=new List<int>{1}}
        });
        Salaries.AddRange(new[] {
            new Salarie{Id=1,SocieteId=1,Matricule="M001",Nom="AIT AHMED",Prenom="ABDELAZIZ",CIN="CD123",Fonction="Développeur",Statut="Actif",SalaireBase=9000,Primes=1040,DateEntree=DateTime.Today.AddYears(-4)},
            new Salarie{Id=2,SocieteId=1,Matricule="M002",Nom="AIT NICER",Prenom="FATIMA",CIN="CD456",Fonction="Comptable",Statut="Actif",SalaireBase=7800,Primes=700,DateEntree=DateTime.Today.AddYears(-2)},
            new Salarie{Id=3,SocieteId=1,Matricule="M003",Nom="FARES",Prenom="ABDELAAZIZ",CIN="CD789",Fonction="Commercial",Statut="Actif",SalaireBase=6200,Primes=400,DateEntree=DateTime.Today.AddYears(-1)},
            new Salarie{Id=4,SocieteId=2,Matricule="B001",Nom="KHALID",Prenom="HAFSA",CIN="BB111",Fonction="RH",Statut="Actif",SalaireBase=6500,Primes=500,DateEntree=DateTime.Today.AddYears(-3)}
        });
        foreach (var s in Salaries) Contrats.Add(new Contrat { Id = NextId(Contrats), SalarieId = s.Id, TypeContrat = "CDI", Poste = s.Fonction, DateDebut = s.DateEntree, SalaireBase = s.SalaireBase, Statut = "Actif" });
        Familles.Add(new Famille { Id = 1, SalarieId = 1, NomComplet = "AIT AHMED YASSINE", Lien = "Enfant", DateNaissance = DateTime.Today.AddYears(-6) });
        Absences.Add(new Absence { Id = 1, SalarieId = 2, Type = "Congé payé", DateDebut = DateTime.Today.AddDays(-5), DateFin = DateTime.Today.AddDays(-3), Jours = 3, Statut = "Validée" });
        Rubriques.AddRange(new[] { new Rubrique { Id = 1, Code = "BASE", Libelle = "Salaire de base", Type = "Gain", Taux = 0 }, new Rubrique { Id = 2, Code = "CNSS", Libelle = "Cotisation CNSS", Type = "Retenue", Taux = 4.48 }, new Rubrique { Id = 3, Code = "AMO", Libelle = "Cotisation AMO", Type = "Retenue", Taux = 2.26 } });
        ResetPermissions();
        CurrentUser = Users[0]; CurrentSociete = Societes[0]; GenerateBulletins(false);
        Audits.Add(new AuditLog(DateTime.Now, "system", "Initialisation des données"));
    }
}

public class Societe { public int Id { get; set; } public string RaisonSociale { get; set; } = ""; public string Code { get; set; } = ""; public string Forme { get; set; } = ""; public string Ville { get; set; } = ""; public string Adresse { get; set; } = ""; public string ICE { get; set; } = ""; public string CNSS { get; set; } = ""; public bool Principale { get; set; } public double TauxCnss { get; set; } public double TauxAmo { get; set; } public double TauxIr { get; set; } }
public class User { public int Id { get; set; } public string Username { get; set; } = ""; public string Password { get; set; } = ""; public string FullName { get; set; } = ""; public string Role { get; set; } = ""; public bool Active { get; set; } public List<int> SocieteIds { get; set; } = new(); }
public class Salarie { public int Id { get; set; } public int SocieteId { get; set; } public string Matricule { get; set; } = ""; public string Nom { get; set; } = ""; public string Prenom { get; set; } = ""; public string CIN { get; set; } = ""; public string Fonction { get; set; } = ""; public string Statut { get; set; } = ""; public double SalaireBase { get; set; } public double Primes { get; set; } public DateTime DateEntree { get; set; } public string NomComplet => Nom + " " + Prenom; }
public class Contrat { public int Id { get; set; } public int SalarieId { get; set; } public string TypeContrat { get; set; } = ""; public DateTime DateDebut { get; set; } public DateTime? DateFin { get; set; } public string Poste { get; set; } = ""; public double SalaireBase { get; set; } public string Statut { get; set; } = ""; }
public class Famille { public int Id { get; set; } public int SalarieId { get; set; } public string NomComplet { get; set; } = ""; public string Lien { get; set; } = ""; public DateTime DateNaissance { get; set; } }
public class Absence { public int Id { get; set; } public int SalarieId { get; set; } public string Type { get; set; } = ""; public DateTime DateDebut { get; set; } public DateTime DateFin { get; set; } public double Jours { get; set; } public string Statut { get; set; } = ""; }
public class Rubrique { public int Id { get; set; } public string Code { get; set; } = ""; public string Libelle { get; set; } = ""; public string Type { get; set; } = ""; public double Taux { get; set; } }
public class Bulletin { public int Id { get; set; } public int SocieteId { get; set; } public int SalarieId { get; set; } public string Periode { get; set; } = ""; public double Brut { get; set; } public double Cnss { get; set; } public double Amo { get; set; } public double Ir { get; set; } public double Retenues { get; set; } public double NetAPayer { get; set; } public double CotisationsPatronales { get; set; } public double CoutGlobal { get; set; } public DateTime DateGeneration { get; set; } }
public record AuditLog(DateTime Date, string User, string Action);
public class SocieteRow { public int Id { get; set; } public string Code { get; set; } public string RaisonSociale { get; set; } public string Forme { get; set; } public string Ville { get; set; } public string ICE { get; set; } public bool Principale { get; set; } public SocieteRow(Societe s) { Id = s.Id; Code = s.Code; RaisonSociale = s.RaisonSociale; Forme = s.Forme; Ville = s.Ville; ICE = s.ICE; Principale = s.Principale; } }
public class UserRow { public int Id { get; set; } public string Username { get; set; } public string FullName { get; set; } public string Role { get; set; } public bool Active { get; set; } public string Societes { get; set; } public UserRow(User u, IEnumerable<Societe> societes) { Id = u.Id; Username = u.Username; FullName = u.FullName; Role = u.Role; Active = u.Active; Societes = string.Join(", ", societes.Where(s => u.SocieteIds.Contains(s.Id)).Select(s => s.Code)); } }
public class BulletinRow { public int Id { get; set; } public string Periode { get; set; } = ""; public string Salarie { get; set; } = ""; public double Brut { get; set; } public double Cnss { get; set; } public double Amo { get; set; } public double Ir { get; set; } public double Retenues { get; set; } public double NetAPayer { get; set; } public DateTime DateGeneration { get; set; } }
public class PermissionRow { public string Role { get; set; } public string Module { get; set; } public bool Acces { get; set; } = true; public bool Lecture { get; set; } = true; public bool Creation { get; set; } = true; public bool Modification { get; set; } = true; public bool Suppression { get; set; } = false; public PermissionRow(string role, string module) { Role = role; Module = module; if (role == "Lecteur") { Creation = Modification = Suppression = false; } } public void SetAll(bool v) { Acces = Lecture = Creation = Modification = Suppression = v; } }
