using System.Collections;
using Gacha.gameplay;
using UnityEngine;

public class GachaCoinInsert : MonoBehaviour
{
    [SerializeField] GachaMachine gachaMachine;
    [SerializeField] AudioSource audioSource;
    [SerializeField] GameObject coinObject;
    [SerializeField] Transform coinStartPoint;
    [SerializeField] Transform coinEndPoint;
    [SerializeField] float moveDuration = 0.3f;

    Coroutine moveCoroutine;

    public bool AddCoins()
    {
        if (gachaMachine.EnoughCoins())
        {
            return false;
        }

        gachaMachine.AddCurrentCoins();
        audioSource.clip = SoundManager.Instance.GetClip(SoundType.InsertCoinRandom);
        audioSource.Play();

        if (moveCoroutine != null)
        {
            StopCoroutine(moveCoroutine);
        }
        moveCoroutine = StartCoroutine(MoveCoin());

        return true;
    }

    IEnumerator MoveCoin()
    {
        // Enable the coin and set position
        coinObject.SetActive(true);
        coinObject.transform.position = coinStartPoint.position;

        // Set random X rotation while keeping Y and Z
        Vector3 startEuler = coinStartPoint.rotation.eulerAngles;
        startEuler.x = Random.Range(0f, 360f);
        coinObject.transform.rotation = Quaternion.Euler(startEuler);

        Vector3 startPos = coinStartPoint.position;
        Vector3 endPos = coinEndPoint.position;
        Quaternion startRot = coinObject.transform.rotation;
        Quaternion endRot = coinEndPoint.rotation;

        float elapsed = 0f;

        while (elapsed < moveDuration)
        {
            if (coinObject == null) yield break;

            float t = elapsed / moveDuration;
            coinObject.transform.position = Vector3.Lerp(startPos, endPos, t);
            coinObject.transform.rotation = Quaternion.Slerp(startRot, endRot, t);
            elapsed += Time.deltaTime;
            yield return null;
        }

        coinObject.transform.position = endPos;
        coinObject.transform.rotation = endRot;
        coinObject.SetActive(false);
    }
}
