# ROLE:
Unity3D Code Extractor by File Path

# GOAL:
Extract code snippets from provided file path

# FILEPATH:
@filePathes@

# EXAMPLE:
## example filepathes:
-ZombieRoyale/Scripts/Clients/PlayerCharacterController.cs:
## example solution:

To link the chest functionality to the player character, you need to add interaction detection in the `PlayerCharacterController` and modify the `ChestController` to respond to player proximity. Here's how you can do it:

PlayerCharacterController.cs

Add a reference to the `ChestController` and a detection mechanism for the chest in the player's update loop.

```csharp
using UnityEngine;

public class PlayerCharacterController : MonoBehaviour
{
    ...

    public float interactionRange = 2f;
    private ChestController currentChest;

    void Update()
    {
        Move();
        Turn();
        CheckForChest();
        InteractWithChest();
    }

    void CheckForChest()
    {
        Collider[] hitColliders = Physics.OverlapSphere(transform.position, interactionRange);
        foreach (var hitCollider in hitColliders)
        {
            if (hitCollider.CompareTag("Chest"))
            {
                currentChest = hitCollider.GetComponent<ChestController>();
                return;
            }
        }
        currentChest = null;
    }

    void InteractWithChest()
    {
        if (Input.GetKeyDown(KeyCode.E) && currentChest != null)
        {
            currentChest.ToggleChest();
        }
    }
}
```

ChestController.cs

Add a public method to toggle the chest state, so it can be called by the player.

```csharp
using UnityEngine;

public class ChestController : MonoBehaviour
{
    public GameObject lid; // The lid of the chest
    public float openAngle = 90f;
    public float openSpeed = 2f;
    private bool isOpen = false;

    ...

    public void ToggleChest()
    {
        if (isOpen)
        {
            StartCoroutine(CloseChest());
        }
        else
        {
            StartCoroutine(OpenChest());
        }
    }

    System.Collections.IEnumerator OpenChest()
    {
        isOpen = true;
        Quaternion targetRotation = Quaternion.Euler(openAngle, 0, 0);
        while (Quaternion.Angle(lid.transform.localRotation, targetRotation) > 0.1f)
        {
            lid.transform.localRotation = Quaternion.Lerp(lid.transform.localRotation, targetRotation, Time.deltaTime * openSpeed);
            yield return null;
        }
        lid.transform.localRotation = targetRotation;
    }

    System.Collections.IEnumerator CloseChest()
    {
        isOpen = false;
        Quaternion targetRotation = Quaternion.Euler(0, 0, 0);
        while (Quaternion.Angle(lid.transform.localRotation, targetRotation) > 0.1f)
        {
            lid.transform.localRotation = Quaternion.Lerp(lid.transform.localRotation, targetRotation, Time.deltaTime * openSpeed);
            yield return null;
        }
        lid.transform.localRotation = targetRotation;
    }
}
```

Setup in Unity:

1. **PlayerCharacterController.cs**:
    - Attach this script to your player GameObject.
    - Set the `interactionRange` value as needed.

2. **ChestController.cs**:
    - Attach this script to the chest GameObject.
    - Tag the chest GameObject with "Chest".
    - Drag the lid GameObject into the `lid` field in the script component in the Inspector.

This setup allows the player to interact with a chest within a certain range by pressing the "E" key.


## example output:
```csharp
using UnityEngine;

public class PlayerCharacterController : MonoBehaviour
{
...

    public float interactionRange = 2f;
    private ChestController currentChest;

    void Update()
    {
        Move();
        Turn();
        CheckForChest();
        InteractWithChest();
    }

    void CheckForChest()
    {
        Collider[] hitColliders = Physics.OverlapSphere(transform.position, interactionRange);
        foreach (var hitCollider in hitColliders)
        {
            if (hitCollider.CompareTag("Chest"))
            {
                currentChest = hitCollider.GetComponent<ChestController>();
                return;
            }
        }
        currentChest = null;
    }

    void InteractWithChest()
    {
        if (Input.GetKeyDown(KeyCode.E) && currentChest != null)
        {
            currentChest.ToggleChest();
        }
    }
}
```

# SOLUTION: