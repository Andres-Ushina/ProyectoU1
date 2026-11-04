/* COMENTARIO EXPLICATIVO: Consultas de exploracion para películas populares, clientes frecuentes y copias actualmente disponibles. */
/* Consulta 1: diez películas con más alquileres. */
/* Une película, copias y rentas; COUNT obtiene popularidad histórica. */
SELECT TOP (10) f.title AS Pelicula, COUNT(r.rental_id) AS CantidadAlquileres
FROM dbo.film AS f
INNER JOIN dbo.inventory AS i ON i.film_id = f.film_id
INNER JOIN dbo.rental AS r ON r.inventory_id = i.inventory_id
WHERE f.is_active = 1
GROUP BY f.film_id, f.title
ORDER BY COUNT(r.rental_id) DESC, f.title;
GO

/* Consulta 2: cinco clientes con más alquileres. */
/* Agrupa rentas por cliente para identificar los cinco mas frecuentes. */
SELECT TOP (5)
       CONCAT(c.first_name, N' ', c.last_name) AS Cliente,
       COUNT(r.rental_id) AS CantidadAlquileres
FROM dbo.customer AS c
INNER JOIN dbo.rental AS r ON r.customer_id = c.customer_id
GROUP BY c.customer_id, c.first_name, c.last_name
ORDER BY COUNT(r.rental_id) DESC, c.last_name, c.first_name;
GO

/* Consulta 3: películas que actualmente tienen una o más copias disponibles. */
/* Cuenta solo copias activas que no tienen un alquiler sin devolver. */
SELECT TOP (10) f.title AS Pelicula, COUNT(i.inventory_id) AS CopiasDisponibles
FROM dbo.film AS f
INNER JOIN dbo.inventory AS i ON i.film_id = f.film_id AND i.is_active = 1
WHERE f.is_active = 1
  AND NOT EXISTS
      (SELECT 1 FROM dbo.rental AS r
       WHERE r.inventory_id = i.inventory_id AND r.return_date IS NULL)
GROUP BY f.film_id, f.title
HAVING COUNT(i.inventory_id) > 0
ORDER BY f.title;
GO
