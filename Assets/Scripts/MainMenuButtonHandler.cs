using UnityEngine;
using UnityEngine.SceneManagement;

public class MainMenuButtonHandler : MonoBehaviour
{
    public static void OpenParkingWarden() {
        SceneManager.LoadScene("Scenes/ParkingWarden");
    }

    public static void OpenPacmanScene() {
        SceneManager.LoadScene("Pacman/Scenes/MedPacman");
    }

    public static void OpenPongScene() {
        SceneManager.LoadScene("Pong/MedPong");
    }

    public static void OpenEVPScene() {
        SceneManager.LoadScene("EVP/EVP");
    }

    public static void OpenNeeuroScene() {
        SceneManager.LoadScene("Neeuro/Sample/Scenes/demo");
    }
}
