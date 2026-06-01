# Guía del Administrador — Lienzo

## 1. Gestión de Usuarios

### Crear usuario manual
- Ir a `/admin/users` → botón **Nuevo Usuario**
- Completar nombre, email, contraseña y seleccionar rol (Admin / Docente / Estudiante)
- El usuario queda activo inmediatamente

### Sincronizar docentes
- En `/admin/users` → botón **Sincronizar Docentes**
- Importa profesores desde el sistema académico externo. No duplica si ya existen (identifica por email)

### Editar / desactivar usuario
- Hacer clic en el usuario en la tabla → editar datos o marcar como inactivo
- Usuarios inactivos no pueden iniciar sesión

---

## 2. Gestión de Aulas (CRUD)

- `/admin/classrooms` → tabla con todas las aulas
- **Crear**: botón **Nuevo Aula** → nombre, código, edificio, piso, capacidad, tipo, descripción, características (separadas por coma)
- **Editar**: clic en el aula → modificar cualquier campo
- **Imagen**: subir JPG/PNG/WebP con previsualización
- **Sincronizar**: botón **Sincronizar Aulas** → importa desde sistema externo

### Tipos de aula
- `AulaRegular`, `Laboratorio`, `SalaConferencias`, `SalonActos`, `SalaEstudio`

### Características disponibles
- `projector`, `wifi`, `ac`, `computers`, `whiteboard`, `chairs`

---

## 3. Gestión de Edificios

- `/admin/buildings` → CRUD completo (nombre, código, dirección, número de pisos)
- Botón **Sincronizar Edificios** para importar desde sistema externo

---

## 4. Periodos Académicos

- `/admin/periodos` → CRUD de periodos (nombre, fecha inicio, fecha fin, año)
- **Sincronizar Tipos** y **Sincronizar Periodos** desde sistema externo

---

## 5. Carreras

- `/admin/carreras` → CRUD de carreras (nombre, código)
- **Sincronizar** desde sistema externo

---

## 6. Actividades (Materias)

- `/admin/actividades` → tabla con búsqueda y exportación Excel/PDF
- **Crear**: nombre, código, periodo, carrera, docentes asignados (búsqueda con selección múltiple), horario opcional (aula, día, hora inicio/fin)

### Horario opcional por actividad
- Seleccionar aula, día de la semana, hora inicio y fin
- Al guardar, se crea automáticamente la reserva recurrente correspondiente

---

## 7. Reservaciones

- `/admin/reservations` → todas las reservas del sistema

### Acciones disponibles
- **Aprobar** / **Rechazar** individual o múltiple (seleccionar con checkbox y usar botón de acción masiva)
- **Filtros**: estado, búsqueda por aula/usuario
- **Exportar** a Excel y PDF
- **Ordenar** por cualquier columna

### Flujo de estado
```
Pendiente → Aprobada → Completada
Pendiente → Rechazada
Aprobada → Cancelada
Pendiente → Cancelada
```

---

## 8. Mantenimiento

- `/admin/maintenance` → bloquear aulas para mantenimiento

### Crear bloqueo
- Seleccionar aula, fecha/hora inicio, fecha/hora fin, motivo
- El sistema **cancela automáticamente** las reservas solapadas y notifica a los afectados

### Finalizar bloqueo
- Botón **Finalizar** en un bloque activo → libera el aula antes de lo previsto

### Visualización en frontend
- Las aulas en mantenimiento muestran un badge amarillo en `ClassroomDetail`
- El botón **Reservar ahora** se deshabilita mientras esté activo el bloqueo

---

## 9. Encuestas

- `/admin/surveys` → tres pestañas

### Respuestas
- Tabla de todas las respuestas con calificaciones (condición, equipo, limpieza, general) y comentarios
- Filtro por **edificio**

### Resumen por Aula
- Promedio de cada categoría por aula
- Estrellas visuales
- Filtro por **edificio**

### Analítica
- Tarjetas KPI: Total evaluaciones, Promedio general, Aula mejor evaluada, Aula peor evaluada
- Gráfico de distribución de calificaciones
- Comparativa por categoría (condición, equipo, limpieza)
- Top 5 y Bottom 5 aulas
- **Exportar PDF** con tablas de resumen, distribución y ranking

---

## 10. Feriados

- `/admin/holidays` → agregar/quitar fechas feriadas
- En feriados no se permiten reservas y el horario los muestra como bloqueados

---

## 11. Reportes

- `/admin/reports` → tres tipos de reporte

### Reporte de Uso
- Total reservas, horas, índice de cancelación agrupado por aula
- Filtro por rango de fechas

### Métricas de Demanda
- Horas pico (qué horas del día tienen más reservas)
- Demanda por tipo de aula

### Uso por Propuesta / Docente
- Desglose de uso agrupado por propuesta académica o por docente individual
- Filtro por rango de fechas

---

## 12. Sidebar de Administración

El menú lateral izquierdo tiene dos secciones:

| Sección | Ítems |
|---------|-------|
| Principal | Reservaciones, Aulas, Edificios, Reportes, Mantenimiento, Encuestas, Feriados |
| Académico | Periodos, Carreras, Actividades, Usuarios |

El sidebar se puede colapsar (íconos solamente) con el botón `<` en la parte superior.

---

## 13. Notificaciones en Tiempo Real

- Las notificaciones aparecen automáticamente vía SignalR
- Ícono de campana en el header con contador de no leídas
- Eventos que generan notificación: cambio de estado de reserva, cancelación por mantenimiento, etc.
