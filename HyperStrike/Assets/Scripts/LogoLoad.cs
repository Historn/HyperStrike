using System.Collections;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class LogoLoad : MonoBehaviour
{
    Image logo;

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {

        logo = GetComponent<Image>();
        logo.fillAmount = 0f;
    }

    // Update is called once per frame
    void Update()
    {
        if (logo != null && logo.fillAmount < 1.0f)
        {
            logo.fillAmount += Time.deltaTime;
        }
        else StartCoroutine(Wait());

        if (Input.GetKeyDown(KeyCode.Space)) SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex + 1);
    }

    IEnumerator Wait()
    {
        yield return new WaitForSeconds(2);
        SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex + 1);
    }
}
