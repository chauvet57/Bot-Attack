using UnityEngine;
using UnityEngine.Networking;

[RequireComponent(typeof(WeaponManager))]
public class PlayerShoot : NetworkBehaviour
{
    private WeaponManager weaponManager;
    private PlayerWeapon currentWeapon;

    [SerializeField]
    private Camera cam;
    [SerializeField]
    private LayerMask mask;

    
    void Start()
    {
       if(cam == null)
        {
            Debug.LogError("Pas de caméra référencée.");
            this.enabled = false;
        }

       weaponManager = GetComponent<WeaponManager>();
    }

    
    void Update()
    {
        if(PauseMenu.isOn == true)
        {
            return;
        }

        currentWeapon = weaponManager.GetCurrentWeapon();

        if(currentWeapon.fireRate <= 0f) { 
            if (Input.GetButtonDown("Fire1"))
            {
                Shoot();
            }
        }
        else
        {
            if (Input.GetButtonDown("Fire1"))
            {
                InvokeRepeating("Shoot", 0f, 1f/currentWeapon.fireRate);
            }
            else if (Input.GetButtonUp("Fire1"))
            {
                CancelInvoke("Shoot");
            }
        }
    }
    //fonction appelé sur le serveur quand un joueur tir
    [Command]
    void CmdOnShoot()
    {
        RpcDoShootEffect();
    }

    //fait apparaitre les effets de tir sur tous les clients
    [ClientRpc]
    void RpcDoShootEffect()
    {
        weaponManager.GetCurrentGraphics().muzzleFlash.Play();
    }

    [Command]
    void CmdOnHit(Vector3 _pos, Vector3 _normal)
    {
        RpcDoHitEffect(_pos, _normal);
    }

    [ClientRpc]
    void RpcDoHitEffect(Vector3 _pos, Vector3 _normal)
    {
        GameObject _hitEffect = (GameObject)Instantiate(weaponManager.GetCurrentGraphics().hitEffectPrefab, _pos, Quaternion.LookRotation(_normal));
        Destroy(_hitEffect, 2f);
    }

    [Client]
    private void Shoot()
    {
        if (!isLocalPlayer)
        {
            return;
        }

        //fonction appelée lors d'un tir sur tous les clients
        CmdOnShoot();

        //Debug.Log("tir effectué");
        RaycastHit _hit;
        if (Physics.Raycast(cam.transform.position, cam.transform.forward, out _hit, currentWeapon.range, mask))
        {
            
            if (_hit.collider.tag == "Player")
            {
                CmdPlayerShoot(_hit.collider.name, currentWeapon.damage);
                //Debug.Log(_hit.collider.name + "a été touché.");
            }

            CmdOnHit(_hit.point, _hit.normal);
        }
    }

    [Command]
    
     void CmdPlayerShoot(string _playerID, int _damage)
    {
        Debug.Log( _playerID + "a été touché.");

        Player _player = GameManager.GetPlayer(_playerID);
        _player.RpcTakeDamage(_damage);
    }
}
