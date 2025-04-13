using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.XR.Interaction.Toolkit;
using UnityEngine.XR.Interaction.Toolkit.Interactors;

[AddComponentMenu("Nokobot/Modern Guns/Simple Shoot")]
public class SimpleShoot : MonoBehaviour
{
    [Header("Prefab Refrences")]
    public GameObject bulletPrefab;
    public GameObject casingPrefab;
    public GameObject muzzleFlashPrefab;

    [Header("Location Refrences")]
    [SerializeField] private Animator gunAnimator;
    [SerializeField] private Transform barrelLocation;
    [SerializeField] private Transform casingExitLocation;

    [Header("Settings")]
    [Tooltip("Specify time to destory the casing object")] [SerializeField] private float destroyTimer = 2f;
    [Tooltip("Bullet Speed")] [SerializeField] private float shotPower = 500f;
    [Tooltip("Casing Ejection Speed")] [SerializeField] private float ejectPower = 150f;

    // public bool pullTrigger = false;
    public AudioSource audioSource;
    public AudioClip fireSound;
    public AudioClip reloadSound;
    public AudioClip noAmmoSound;
    public Magazine magazine;
    public XRSocketInteractor socketInteractor;
    // public DisableMagazineCollision magazineColider;
    private bool hasSlide = true;

    void Start()
    {
        if (barrelLocation == null)
            barrelLocation = transform;

        if (gunAnimator == null)
            gunAnimator = GetComponentInChildren<Animator>();
        if (socketInteractor != null)
        {
            socketInteractor.selectEntered.AddListener(AddMagazine); 
            socketInteractor.selectExited.AddListener(RemoveMagazine);
        }

        DisableMagazineCollisions();

    }

    public void AddMagazine(SelectEnterEventArgs args)
    {
        // Get the Magazine component from the interactable object
        magazine = args.interactableObject.transform.GetComponent<Magazine>();

        // Play the reload sound
        if (audioSource != null && reloadSound != null)
        {
            audioSource.PlayOneShot(reloadSound);
        }
        magazine.tag = "CurrentMagazine";

        hasSlide = false;

    }


    public void RemoveMagazine(SelectExitEventArgs args)
    {
        if (audioSource != null && reloadSound != null)
        {
            audioSource.PlayOneShot(reloadSound);
        }

        // magazineColider.currentMagazine = null;
        magazine.tag = "Magazine";
        magazine = null;
    }

    public void Slide()
    {
        hasSlide = true;
        if (audioSource != null && reloadSound != null)
        {
            audioSource.PlayOneShot(reloadSound);
        }
    }

    public void PullTheTrigger()
    {
        if (magazine && magazine.nunmberOfBullet > 0 && hasSlide)
        {
            gunAnimator.SetTrigger("Fire");
        }
        else
        {
            audioSource.PlayOneShot(noAmmoSound);
        }
    }

    //This function creates the bullet behavior
    void Shoot()
    {
        magazine.nunmberOfBullet--;
        audioSource.PlayOneShot(fireSound);

        if (muzzleFlashPrefab)
        {
            //Create the muzzle flash
            GameObject tempFlash;
            tempFlash = Instantiate(muzzleFlashPrefab, barrelLocation.position, barrelLocation.rotation);

            //Destroy the muzzle flash effect
            Destroy(tempFlash, destroyTimer);
        }

        //cancels if there's no bullet prefeb
        if (!bulletPrefab)
        { return; }

        // Create a bullet and add force on it in direction of the barrel
        Instantiate(bulletPrefab, barrelLocation.position, barrelLocation.rotation).GetComponent<Rigidbody>().AddForce(barrelLocation.forward * shotPower);

    }

    //This function creates a casing at the ejection slot
    void CasingRelease()
    {
        //Cancels function if ejection slot hasn't been set or there's no casing
        if (!casingExitLocation || !casingPrefab)
        { return; }

        //Create the casing
        GameObject tempCasing;
        tempCasing = Instantiate(casingPrefab, casingExitLocation.position, casingExitLocation.rotation) as GameObject;
        //Add force on casing to push it out
        tempCasing.GetComponent<Rigidbody>().AddExplosionForce(Random.Range(ejectPower * 0.7f, ejectPower), (casingExitLocation.position - casingExitLocation.right * 0.3f - casingExitLocation.up * 0.6f), 1f);
        //Add torque to make casing spin in random direction
        tempCasing.GetComponent<Rigidbody>().AddTorque(new Vector3(0, Random.Range(100f, 500f), Random.Range(100f, 1000f)), ForceMode.Impulse);

        //Destroy casing after X seconds
        Destroy(tempCasing, destroyTimer);
    }

    public void DisableMagazineCollisions()
    {
        if (magazine != null && magazine.GetComponent<Rigidbody>() != null)
        {
            magazine.GetComponent<Rigidbody>().detectCollisions = false;
        }
    }

    public void DisableMagazineCollisionsAddDelay()
    {
        if (magazine != null && magazine.GetComponent<Rigidbody>() != null)
        {
            StartCoroutine(DisableCollisionsWithDelay(0.1f));
        }
    }

    public void EnableMagazineCollisions()
    {
        if (magazine != null && magazine.GetComponent<Rigidbody>() != null)
        {
            magazine.GetComponent<Rigidbody>().detectCollisions = true;
        }
    }

    public void EnableMagazineCollisionsAddDelay()
    {
        if (magazine != null && magazine.GetComponent<Rigidbody>() != null)
        {
            StartCoroutine(EnableCollisionsWithDelay(0.05f));
        }
    }

    private IEnumerator DisableCollisionsWithDelay(float delay)
    {
        yield return new WaitForSeconds(delay);

        if (magazine != null && magazine.GetComponent<Rigidbody>() != null)
        {
            magazine.GetComponent<Rigidbody>().detectCollisions = false;
        }
    }

    private IEnumerator EnableCollisionsWithDelay(float delay)
    {
        yield return new WaitForSeconds(delay);

        if (magazine != null && magazine.GetComponent<Rigidbody>() != null)
        {
            magazine.GetComponent<Rigidbody>().detectCollisions = true;
        }
    }
}
