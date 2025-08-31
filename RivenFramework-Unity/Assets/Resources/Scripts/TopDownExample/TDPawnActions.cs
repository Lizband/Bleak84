//===================== (Neverway 2024) Written by Liz M. =====================
//
// Purpose:
// Notes:
//
//=============================================================================

using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using RivenFramework;

public class TDPawnActions : PawnActions
{
    //=-----------------=
    // Public Variables
    //=-----------------=


    //=-----------------=
    // Private Variables
    //=-----------------=
    private GameObject viewCamera;


    //=-----------------=
    // Reference Variables
    //=-----------------=


    //=-----------------=
    // Mono Functions
    //=-----------------=
    

    //=-----------------=
    // Internal Functions
    //=-----------------=


    //=-----------------=
    // External Functions
    //=-----------------=
    /// <summary>
    /// Make the pawn move, using velocity, in a specified direction
    /// </summary>
    /// <param name="_pawn">A reference to the owning pawn</param>
    /// <param name="_rigidbody">A reference to the owning rigidbody</param>
    /// <param name="_direction">The direction to move in (x-axis is left/right, y-axis is forward/backward, and z-axis is up/down (which is only really used for flying enemies))</param>
    /// <param name="_speed">The speed to move the pawn at (set this to 0 to just use the stats movement speed)</param>
    public void Move(TDPawn _pawn, Vector3 _direction, float _speed=0)
    {
        if (_speed == 0)
        {
            _speed = ((TDPawnStats)_pawn.currentStats).movementSpeed;
        }

        var rigidbody = _pawn.GetComponent<Rigidbody2D>();
        
        // Make sure that the axis passed for the direction are always relative to the direction the pawn is facing
        var localMoveDirection = _pawn.transform.right * _direction.x + _pawn.transform.up * _direction.y + _pawn.transform.forward * _direction.z;
        var currentVelocity = rigidbody.velocity;
        
        // Get desired velocities
        var desiredGroundVelocity = localMoveDirection.normalized * _speed;
        
        // Define acceleration rates
        var groundAccelerationRate = ((TDPawnStats)_pawn.currentStats).groundAccelerationRate;
        
        // Ground Movement
        rigidbody.drag = ((TDPawnStats)_pawn.currentStats).groundDrag;
        // if current is less than target and target is positive, or current is greater than target and target is negative
        if (currentVelocity.x < desiredGroundVelocity.x && desiredGroundVelocity.x > 0f || currentVelocity.x > desiredGroundVelocity.x && desiredGroundVelocity.x < 0f )
        {
            rigidbody.velocity += new Vector2(desiredGroundVelocity.x*groundAccelerationRate, 0);
        }
        if (currentVelocity.y < desiredGroundVelocity.y && desiredGroundVelocity.y > 0f || currentVelocity.y > desiredGroundVelocity.y && desiredGroundVelocity.y < 0f )
        {
            rigidbody.velocity += new Vector2(0, desiredGroundVelocity.y*groundAccelerationRate);
        }
    }
    
    /// <summary>
    /// TODO Make the pawn move in a direct path to a specified position
    /// </summary>
    /// <param name="_position"></param>
    public void MoveTo(Vector3 _position)
    {
        
    }
    
    /// <summary>
    /// TODO Make the pawn path-find it's way to a specified position
    /// </summary>
    /// <param name="_position"></param>
    public void MoveToSmart(Vector3 _position)
    {
        
    }
    
    /// <summary>
    /// Make the pawn turn to face a specified amount
    /// </summary>
    /// <param name="_pawn">A reference to the root of the pawn (this is needed to rotate the body to look left and right)</param>
    /// <param name="_viewPoint">A reference to the object that represents the head of the pawn (this is needed to rotate the head to look up and down)</param>
    /// <param name="_direction">The direction to rotate in (x-axis is left/right, y-axis is up/down)</param>
    public void FaceTowardsDirection(TDPawn _pawn, Transform _viewPoint, Vector2 _direction)
    {
        _viewPoint.localRotation = Quaternion.Euler(_direction.x, 0, 0); // Rotate the head for up/down
        _pawn.transform.rotation = Quaternion.Euler(0, _direction.y, 0); // Rotate the body for left/right
    }
    
    /// <summary>
    /// Make the pawn face at a specified point
    /// </summary>
    /// <param name="_pawn">A reference to the root of the pawn (this is needed to rotate the body to look left and right)</param>
    /// <param name="_viewPoint">A reference to the object that represents the head of the pawn (this is needed to rotate the head to look up and down)</param>
    /// <param name="_position"></param>
    /// <param name="_speed"></param>
    public void FaceTowardsPosition(TDPawn _pawn, Transform _viewPoint, Vector3 _position, float _speed)
    {
        var vectorToTarget = _pawn.transform.position - _position;

        // Rotate the body for left/right
        var bodyLookRotation = Mathf.Atan2(vectorToTarget.x, vectorToTarget.z) * Mathf.Rad2Deg;
        _pawn.transform.rotation = Quaternion.Euler(0, bodyLookRotation+180, 0);
        
        // Rotate the head for up/down
        var headLookRotation = Quaternion.LookRotation(vectorToTarget, _pawn.transform.up).eulerAngles;
        var desiredRotation = new Vector3(-headLookRotation.x, headLookRotation.y + 180, headLookRotation.z);
        _viewPoint.transform.eulerAngles = desiredRotation;
    }

    /// <summary>
    /// TODO
    /// </summary>
    public void Interact(TDPawn _pawn, GameObject _interactionTrigger, Transform _viewPoint)
    {
        var interaction = Object.Instantiate(_interactionTrigger, _viewPoint);
        interaction.transform.GetChild(0).GetComponent<VolumeTriggerInteraction>().owningPawn = _pawn;
        Object.Destroy(interaction,  0.2f);
    }

    /// <summary>
    /// TODO
    /// </summary>
    /// <param name="_action"></param>
    public void ItemUseAction(Pawn_Inventory _inventory, int _action = 0, string _mode = "press")
    {
        var item = _inventory.GetComponentInChildren<Item>(false);
        if (item is null) return;

        switch (_action)
        {
            case 0:
                item.UsePrimary(_mode);
                break;
            case 1:
                item.UseSecondary(_mode);
                break;
            case 2:
                item.UseTertiary(_mode);
                break;
        }
    }

    /// <summary>
    /// TODO
    /// </summary>
    public void SwitchItem()
    {
        
    }

    public void EnableViewCamera(TDPawn _pawn, bool _setActive)
    {
        if (viewCamera is null)
        {
            // Try to get a view camera
            viewCamera =_pawn.GetComponentInChildren<Camera>(true).gameObject;
            if (viewCamera is null) return;
        }
        
        viewCamera.SetActive(_setActive);
    }

    /// <summary>
    /// Clears and populates the lists of visible pawns
    /// </summary>
    /// <param name="_pawn"></param>
    /// <param name="_distance"></param>
    public void Look(TDPawn _pawn, float _distance)
    {
        // Clear the list of visible pawns
        _pawn.visiblePawns.Clear();
        _pawn.visibleHostiles.Clear();
        _pawn.visibleAllies.Clear();
        foreach (var target in Physics.OverlapSphere(_pawn.transform.position, _distance))
        {
            // Object is pawn
            var targetPawn = target.GetComponent(typeof(TDPawn)) as TDPawn;
            if (targetPawn)
            {
                if (targetPawn.gameObject == _pawn.gameObject) continue;
                // Pawn is not occluded by something
                //if (!Physics.Raycast(_pawn.viewPoint.transform.position, _pawn.transform.position - target.transform.position, 9999, _pawn.currentStats.groundMask))
                //{
                    // Add it to the list of visible pawns
                    _pawn.visiblePawns.Add(targetPawn);
                    // If it's an enemy, add it to the list of visible hostiles
                    if (_pawn.TDCurrentStats.opposedTeams.Contains(((TDPawnStats)targetPawn.currentStats).team))
                    {
                        _pawn.visibleHostiles.Add(targetPawn);
                    }
                    // If it's a friend, add it to the list of visible allies
                    if (((TDPawnStats)_pawn.currentStats).alliedTeams.Contains(((TDPawnStats)targetPawn.currentStats).team))
                    {
                        _pawn.visibleAllies.Add(targetPawn);
                    }
                //}
            }
        }
    }
    
    public void Listen()
    {
        
    }
    
    public void ThrowPhysProp(TDPawn _pawn)
    {
        _pawn.physObjectAttachmentPoint.attachedObject.GetComponent<Rigidbody2D>().AddForce((viewCamera.transform.forward * ((TDPawnStats)_pawn.currentStats).throwForce));
        _pawn.physObjectAttachmentPoint.attachedObject.GetComponent<Object_PhysPickup>().Drop();
    }

    public TDPawn GetClosest(TDPawn _pawn, List<Pawn> _pawns)
    {
        var closestDistance = 999999f;
        TDPawn closestPawn = null;
        foreach (var target in _pawns)
        {
            var distanceToTarget = Vector3.Distance(_pawn.transform.position, target.transform.position);
            if (distanceToTarget <= closestDistance)
            {
                closestDistance = distanceToTarget;
                closestPawn = ((TDPawn)target);
            }
        }

        return closestPawn;
    }

    public float GetCollectiveAllyCourage(TDPawn _pawn, List<Pawn> _pawns)
    {
        float collectiveAllyCourage = 0;
        foreach (var target in _pawns)
        {
            var distanceToTarget = Vector3.Distance(_pawn.transform.position, target.transform.position);
            if (distanceToTarget <= ((TDPawnStats)_pawn.currentStats).comfortableAllyDistance)
            {
                collectiveAllyCourage += ((TDPawnStats)_pawn.currentStats).courage;
            }
        }
        /*foreach (var VARIABLE in COLLECTION)
        {
            Vector3.Distance(closestAlly.transform.position, _pawn.transform.position) > ((TDS_Stats)_pawn.stats).comfortableAllyDistance
        }*/
        return collectiveAllyCourage;
    }
    
    public void ItemSwapNext(TDPawn _pawn)
    {
        var inventory = _pawn.GetComponentInChildren<Pawn_Inventory>();
        if (inventory is null) return;
        inventory.SwitchNext();
    }

    public void ItemSwapPrevious(TDPawn _pawn)
    {
        var inventory = _pawn.GetComponentInChildren<Pawn_Inventory>();
        if (inventory is null) return;
        inventory.SwitchPreviouse();
    }
}
