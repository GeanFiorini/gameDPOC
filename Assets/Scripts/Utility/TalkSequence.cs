using System.Collections;
using UnityEngine;
using UnityEngine.Events;

public class TalkSequence : MonoBehaviour
{
    [SerializeField] private float _startDelay;
    [SerializeField] private GameObject _continueButton;
    [SerializeField] private GameObject[] _dialogs;
    [SerializeField] private UnityEvent _onSequenceEnd;

    private int _currentDialog;

    private IEnumerator Start()
    {
        yield return new WaitForSeconds(this._startDelay);

        this._currentDialog = 0;

        this._dialogs[0].SetActive(true);
        this._continueButton.SetActive(true);
    }

    public void ContinueTalk()
    {
        this._dialogs[this._currentDialog].SetActive(false);

        if (this._currentDialog == this._dialogs.Length - 1)
        {
            this._onSequenceEnd.Invoke();
        }
        else
        {
            this._currentDialog++;
            this._dialogs[this._currentDialog].SetActive(true);
        }
    }
}
