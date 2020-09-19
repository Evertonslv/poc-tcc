using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MoveObject : MonoBehaviour
{
    //Este codigo e para click/drag gameobject 2D
    //Cameraprojeção deve estar como Orthographic
    //Adicionar um Collider (não 2DCollider) no gameObject

    RaycastHit hit; //Armazena informação que pegou o objeto
    Rigidbody rbTemp;
    Vector3 tempSpeed;
    Vector3 rayEndPoint;
    float tempDistance;
    float distance;
    GameObject tempObject;
    Camera mainCamera;
    float rotXTemp;
    float rotYTemp;

    void Awake()
    {
        mainCamera = Camera.main;
        distance = 4;
    }

    void Update()
    {
        rayEndPoint = transform.position + transform.forward * distance;

        //Quando usar click esquerdo do mouse
        if (Input.GetMouseButtonDown(0) || Input.GetMouseButtonDown(1))
        {
            //converte a posição do click para um ray
            Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);

            //Se o ray acertar (hit) o Collider (não 2DCollider)
            if (Physics.Raycast(ray, out hit))
            {
                if (hit.transform.CompareTag(PropertiesModel.TagMoveObject))
                {
                    if (hit.rigidbody)
                    {
                        hit.rigidbody.useGravity = true;
                        distance = Vector3.Distance(transform.position, hit.point);
                        tempObject = hit.transform.gameObject;
                    }
                }
            }
        }

        distance += Input.GetAxis("Mouse ScrollWheel") * 10.0f;
        distance = Mathf.Clamp(distance, 2.5f, 6);

        if (tempObject)
        {
            rbTemp = tempObject.GetComponent<Rigidbody>();
        }

        if (tempObject && mainCamera)
        {            
            // Se estiver com o mouse clicado
            if (Input.GetMouseButton(0)) {
                rotXTemp = Input.GetAxis("Mouse X") * 10.0f;
                rotYTemp = Input.GetAxis("Mouse Y") * 10.0f;
                tempObject.transform.Rotate(mainCamera.transform.up, -rotXTemp, Space.World);
                tempObject.transform.Rotate(mainCamera.transform.right, rotYTemp, Space.World);
            } 
            else if(Input.GetMouseButton(1))
            {
                rotXTemp = Input.GetAxis("Mouse X");
                rotYTemp = Input.GetAxis("Mouse Y");
                Vector3 newPosiction = new Vector3(rotXTemp, 0, rotYTemp);
                //tempObject.transform.position = tempObject.transform.position + newPosiction.normalized;
                newPosiction = newPosiction.normalized * 2 * Time.deltaTime;
                rbTemp.MovePosition(tempObject.transform.position + newPosiction);
            }
        }

        //Quando solta o click do mouse
        if (Input.GetMouseButtonUp(0) || Input.GetMouseButtonUp(1))
        {
            tempObject = null;
            rbTemp = null;
        }

    }

    void FixedUpdate()
    {
        if (tempObject)
        {
            rbTemp = tempObject.GetComponent<Rigidbody>();
            rbTemp.angularVelocity = new Vector3(0, 0, 0);
            tempSpeed = (rayEndPoint - rbTemp.transform.position);
            tempSpeed.Normalize();
            tempDistance = Vector3.Distance(rayEndPoint, rbTemp.transform.position);
            tempDistance = Mathf.Clamp(tempDistance, 0, 1);
            rbTemp.velocity = Vector3.Lerp(rbTemp.velocity, tempSpeed * 7.5f * tempDistance, Time.deltaTime * 12);
        }
    }
}
