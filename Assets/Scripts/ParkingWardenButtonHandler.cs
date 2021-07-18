using UnityEngine;
using UnityEngine.SceneManagement;

using MedBots;

public class ParkingWardenButtonHandler : MonoBehaviour
{
    public EntropyManager em;

    public void ToggleStartStop() {
        em.Pause = !em.Pause;
    }

    public void Reset() {
        em.Reset();
    }

    public void ToggleBitCountRandomWalk() {
        em.ToggleBitCountRandomWalk();
    }
}
