Para instalar el proyecto backend se deben seguir los siguientes pasos:
<ol>
  <li>Clonar el repositorio</li>
  <li>Posicionarse en la carpeta del proyecyo y montar la imagen de docker con el comando `docker compose up -d`</li>
  <li>Una vez montado el contenedor con el motor de base de datos se debe ingresar al motor con las credenciales `user:sa password:65823_jm`</li>
  <li>Crear una base de datos llamada exactamente `ProdigyPlanning`</li>
  <li>Una vez creada la base de datos se deben correr el script llamado `CreateAdminUser.sql` ubicado en `Proyecto_Istea\database\querys`</li>
  <li>Luego de crear el usuario admin es necesario abrir la solucion del proyecto ubicado en `Proyecto_Istea\ProdigyPlanningAPI\ProdigyPlanningAPI.sln`</li>
  <li>Una vez en el proyecto se recomineda recompilar la solucion para comporbar la descarga de todos los paquetes necesarios</li>
  <li>Abrir la consola administrador de paquetes y correr el siguiendo comando `Update-Database`, esto creara todas las tablas necesarias en la base de datos</li>
  <li>Por ultimo, correr el resto de los scripts guardados en `Proyecto_Istea\database\querys`</li>
</ol>  

Para probar el proyecto se adjunta una version levemente desactualizadad de las colecciones utilizadas para verificar los endpoints utilizando postman.
Las colecciones para importar se encuentran en `Proyecto_Istea\Postman`
Los metodos desactualizados son event/getBanner ya que se encuentra en un estado no complemtante funcional (fue parcheado temporalmente para solucionar un problema del front), y aquellos metodos Get que necesiten recibir parametros, desde la ultima actualizacion estos metodos toman parametros en forma de querystring en el url al hacer la peticion.
