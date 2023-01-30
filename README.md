# MarkdownDocumentation
Console tool for generate markdown documentation at .NET projects

# Etiquetas soportadas:

- Propiedades y campos:
    - `<summary>Resumen</summary>` _(Solo 1)_
    - `<see cref="tipo de dato" />` _(Solo 1)_

- Eventos .NET:
    - `<summary>Resumen</summary>` _(Solo 1)_

- Clases, structs, interfaces (Tipos):
    - `<summary>Resumen</summary>` _(Solo 1)_
    - `<remarks>Descripción</remarks>` _(Solo 1)_

- Métodos:
    - `<summary>Resumen</summary>` _(Solo 1)_
    - `<param name="nombre del parámetro" cref="tipo de dato">Resumen</param>` _(Múltiples, se asume que en el mismo orden que se han definido en el método.)_
    - `<remarks>Descripción</remarks>` _(Solo 1)_
    - `<response code="200 (u otro código HTTP)">Descripción</response>` _(Múltiples)_
    - `<returns cref="tipo de dato">Resumen</returns>` _(Solo 1)_
    - `<example>Información de lo que se espera recibir, bien en formato JSON de los valores requeridos u otros.</example>` _(Solo 1)_
    - `<exception cref="tipo de excepción">Resumen de cuando se produce</exception>` _(Múltiples)_
    - `<uri method="GET|POST|PUT|DELETE|PATCH...">Url relativa de la API</uri>` _(Solo 1)_ _(Solo para endpoints)_
    - `<permission cref="tipo de permiso"/>` _(Múltiples)_ No admite resumen. Indica que se debe poseer uno de los permisos de este listado

# Ejemplo de documentar un endpoint:
```xml
<summary>Este es el resumen del endpoint</summary>
<remarks>Esta es la descripción detallada de qué es lo que hace este endpoint</remarks>
<param name="filters" cref="MyEndpointFiltersDto">Filtros a aplicar en la petición</param>
<param name="cancellationToken" cref="CancellationToken">Token de cancelación provisto por .NET Core</param>
<response code="200">Si el endpoint ha ido correctamente.</response>
<response code="401">No autenticado</response>
<response code="403">No autorizado</response>
<response code="500">Error de servidor</response>
<uri method="POST">/api/myapi/myendpoint</uri>
<example>
  {
    "FilterA": "some text",
    "FilterB": 3,
    "FiltersC": [
      "FilterD": { "Text": "a.." }
    ]
  }
</example>
<permission cref="MyPermissions.Web.PermissionA"/>
<permission cref="MyPermissions.Web.PermissionB"/>
<returns cref="MyEndpointOutputDto">Datos devueltos en caso de que todo vaya correctamente.</returns>
```
```csharp
[HttpPost, Route("api/myapi/myendpoint")]
[Authorize(Permission = MyPermissions.Web.PermissionA)]
[Authorize(Permission = MyPermissions.Web.PermissionB)]
[ProducesResponseType(typeof(MyEndpointOutputDto), (int)HttpStatusCode.OK)]
[ProducesResponseType((int)HttpStatusCode.Unauthorized)]
[ProducesResponseType((int)HttpStatusCode.Forbidden)]
[ProducesResponseType((int)HttpStatusCode.InternalServerError)]
public async Task<IActionResult> GetDataFromMyEndpoint([FromBody] MyEndpointFiltersDto filters, CancellationToken cancellationToken = default)
```