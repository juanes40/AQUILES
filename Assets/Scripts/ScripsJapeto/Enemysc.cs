using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class Enemysc : MonoBehaviour
{
    //Variables para guardar al jugador
    public float visionRadius;
    public float attackRadius;
    public float speed;

    //Variables relacionadas con ataque
    //Prefab disparo
    public GameObject charged6Prefab;
    //Velocidad de ataque (segundos entre ataques)
    public float attackSpeed = 1f;
    bool attacking;
    public bool lookingRight;
    public float playerdistance;
    public Transform zeus;

    //Variables relacionadas con la vida
    //Puntos de vida
    public float maxHealthPoints = 15;
    //Vida actual
    public float healthpoints;


    //Variable para guardar al jugador
    GameObject player;

    // Variable para guardar posici�n inicial
    Vector3 initialPosition, target;

    //Animador y cuerpo cinematico con la rotaci�n en Z congelada
    Animator anim;
    Rigidbody2D rb2d;


    private void Start()
    {

        //Recuperamos al jugador gracias al Tag
        player = GameObject.FindGameObjectWithTag("Player");

        //Guardamos nuestra posici�n inicial
        initialPosition = transform.position;

        anim = GetComponent<Animator>();
        rb2d = GetComponent<Rigidbody2D>();

        healthpoints = maxHealthPoints;

    }
    void Update()
    {

        // Por defecto nuestro target siempre ser� nuestra posici�n inicial 
        Vector3 target = initialPosition;

        //Comprobamos un Raycast del enemigo hasta el jugador 
        RaycastHit2D hit = Physics2D.Raycast(
            transform.position,
            player.transform.position - transform.position,
            visionRadius,
            1 << LayerMask.NameToLayer("Default")
        //  Poner el propio Enemy en una layer distinta a Default para evitar el Raycast
        // Tambi�n poner al objeto Attack y al Prefab Slash un Layer attack 
        // Sino los detectar� como entorno y se mueve atr�s al hacer ataques
        );

        // A qu� se debugea el Raycast 
        Vector3 forward = transform.TransformDirection(player.transform.position - transform.position);
        Debug.DrawRay(transform.position, forward, Color.red);

        LookPlayer();
        //Si el Raycast encuentra al jugador lo ponemos de target 
        if (hit.collider != null)
        {
            if (hit.collider.tag == "Player")
            {
                target = player.transform.position;
            }
        }

        //Calculamos la distancia y direcci�n actual hasta el target 
        float distance = Vector3.Distance(target, transform.position);
        Vector3 dir = (target - transform.position).normalized;

        // Si es el enemigo y est� en rango de ataque para y atarca
        if (target != initialPosition && distance < attackRadius)
        {
            //Ataca
            anim.SetFloat("movX", dir.x);
            anim.SetFloat("movY", dir.y);
            anim.Play("Enemy_Walk", -1, 0); //Congelar animaci�n de caminar

            //Empieza a atacar (Layer en ataque para evitar Raycast)
            if (!attacking) StartCoroutine(Attack(attackSpeed));
        }
        //En caso distinto se mueve hacia �l
        else
        {
            rb2d.MovePosition(transform.position + dir * speed * Time.deltaTime);

            //Al moverse establece la animaci�n movimiento
            anim.speed = 1;
            anim.SetFloat("movX", dir.x);
            anim.SetFloat("movY", dir.y);
            anim.SetBool("Run", true);
        }

        //�ltima comprobaci�n para evitar bugs forzando la posici�n inicial
        if (target == initialPosition && distance < 0.05f)
        {
            transform.position = initialPosition;
            // Y cambiamos la animacii�n de nuevo a Respirar Japeto
            anim.SetBool("Run", false);
        }

        // Y un debug con l�nea hasta el target
        Debug.DrawLine(transform.position, target, Color.green);

        playerdistance = Vector2.Distance(zeus.position, rb2d.position);
    }

    
    public void LookPlayer()
    {
        Vector3 toTurn = transform.localScale;
        if ( playerdistance < visionRadius)
        {
            if (transform.position.x < zeus.position.x && !lookingRight)
            {
                Flip();
                lookingRight = true;
            }
            else if(transform.position.x > zeus.position.x && lookingRight)
            {
                lookingRight = false;
            }
        }
    }

    public void Flip()
    {
        transform.Rotate(0, 180, 0);
    }

    private void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.yellow;
        Gizmos.DrawWireSphere(transform.position, visionRadius);
        Gizmos.DrawWireSphere(transform.position, attackRadius);
    }

    IEnumerator Attack(float seconds)
    {
        attacking = true; //Activar Bandera
        //Si hay objetivo y el prefab es correcto crear instancia
        if (target != initialPosition && charged6Prefab != null)
        {
            Instantiate(charged6Prefab, transform.position, transform.rotation);
            //Esperar los segundos de turno antes de hacer otro ataque
            yield return new WaitForSeconds(seconds);
        }
        attacking = false; //Desactivar bandera
    }

     //Gesti�n de ataque (hecha en 1 sola para ahorrar linea, permite disminuir y destruir)
     public void Attacked()
    {
        healthpoints = healthpoints - 1;
        if (healthpoints <= 0)
        {
            Destroy(gameObject);
            AudioManager.instance.PlayAudio(AudioManager.instance.muerteEnemigo);
        }

    }

}
