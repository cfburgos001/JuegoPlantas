using UnityEditor;
using UnityEditor.SceneManagement;

// Hace que al presionar Play el juego SIEMPRE empiece en la escena Menu, sin importar
// que escena tengas abierta en el editor (asi el orden es: Menu > Seleccion de planta > Dia > Noche > Resultados).
//
// Se puede apagar y prender desde:  Plantas > Al dar Play, empezar siempre desde el Menú
// (apagalo si quieres probar una sola escena, por ejemplo la noche).
[InitializeOnLoad]
public static class InicioDesdeMenu
{
    const string Clave = "Plantas.InicioDesdeMenu";
    const string RutaMenu = "Assets/Scenes/Menu.unity";
    const string TextoMenu = "Plantas/Al dar Play, empezar siempre desde el Menú";

    static InicioDesdeMenu()
    {
        EditorApplication.delayCall += Aplicar;
    }

    static bool Activo
    {
        get { return EditorPrefs.GetBool(Clave, true); }
        set { EditorPrefs.SetBool(Clave, value); }
    }

    public static void Aplicar()
    {
        SceneAsset menu = Activo ? AssetDatabase.LoadAssetAtPath<SceneAsset>(RutaMenu) : null;
        EditorSceneManager.playModeStartScene = menu; // null = empieza en la escena abierta
    }

    [MenuItem(TextoMenu, false, 50)]
    static void Alternar()
    {
        Activo = !Activo;
        Aplicar();
    }

    [MenuItem(TextoMenu, true)]
    static bool MarcarOpcion()
    {
        Menu.SetChecked(TextoMenu, Activo);
        return true;
    }
}
