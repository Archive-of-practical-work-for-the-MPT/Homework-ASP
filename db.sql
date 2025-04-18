-- Создание базы данных
IF NOT EXISTS (SELECT name FROM master.dbo.sysdatabases WHERE name = 'vinyl_store')
BEGIN
    CREATE DATABASE vinyl_store;
END
GO

-- Переинициализация базы данных
USE vinyl_store;
GO

-- Таблица ролей пользователей
CREATE TABLE roles (
    id INT IDENTITY(1,1) PRIMARY KEY,
    name NVARCHAR(50) NOT NULL UNIQUE,
    CONSTRAINT UQ_roles_name UNIQUE (name)
);
GO

-- Таблица пользователей
CREATE TABLE users (
    id INT IDENTITY(1,1) PRIMARY KEY,
    login NVARCHAR(50) NOT NULL UNIQUE,
    password NVARCHAR(255) NOT NULL,
    role_id INT NOT NULL,
    first_name NVARCHAR(50) NOT NULL,
    last_name NVARCHAR(50) NOT NULL,
    birth_date DATE NULL,
    email NVARCHAR(100) NULL,
    phone_number NVARCHAR(20) NULL,
    created_at DATETIME2 DEFAULT GETDATE(),
    updated_at DATETIME2 DEFAULT GETDATE(),
    CONSTRAINT FK_users_roles FOREIGN KEY (role_id) REFERENCES roles(id) ON DELETE CASCADE,
    CONSTRAINT UQ_users_login UNIQUE (login)
);
GO

-- Таблица лейблов звукозаписи
CREATE TABLE labels (
    id INT IDENTITY(1,1) PRIMARY KEY,
    name NVARCHAR(255) NOT NULL UNIQUE,
    CONSTRAINT UQ_labels_name UNIQUE (name)
);
GO

-- Таблица жанров музыки
CREATE TABLE genres (
    id INT IDENTITY(1,1) PRIMARY KEY,
    name NVARCHAR(100) NOT NULL UNIQUE,
    CONSTRAINT UQ_genres_name UNIQUE (name)
);
GO

-- Таблица исполнителей
CREATE TABLE artists (
    id INT IDENTITY(1,1) PRIMARY KEY,
    name NVARCHAR(255) NOT NULL,
    description NVARCHAR(MAX) NULL,
    country NVARCHAR(100) NULL
);
GO

-- Таблица виниловых пластинок
CREATE TABLE vinyls (
    id INT IDENTITY(1,1) PRIMARY KEY,
    title NVARCHAR(255) NOT NULL,
    artist_id INT NOT NULL,
    genre_id INT NOT NULL,
    label_id INT NOT NULL,
    release_year INT NULL,
    cover_image_path NVARCHAR(255) NULL,
    price DECIMAL(10, 2) NOT NULL,
    weight DECIMAL(5, 2) NULL,
    diameter INT NOT NULL DEFAULT 12, -- размер в дюймах (7, 10 или 12)
    rpm INT NOT NULL DEFAULT 33, -- скорость воспроизведения (обычно 33, 45 или 78)
    condition NVARCHAR(50) NULL, -- состояние пластинки (новая, б/у и т.д.)
    quantity_in_stock INT NOT NULL DEFAULT 0,
    CONSTRAINT FK_vinyls_artists FOREIGN KEY (artist_id) REFERENCES artists(id) ON DELETE CASCADE,
    CONSTRAINT FK_vinyls_genres FOREIGN KEY (genre_id) REFERENCES genres(id) ON DELETE CASCADE,
    CONSTRAINT FK_vinyls_labels FOREIGN KEY (label_id) REFERENCES labels(id) ON DELETE CASCADE
);
GO

-- Таблица треков на пластинках
CREATE TABLE tracks (
    id INT IDENTITY(1,1) PRIMARY KEY,
    vinyl_id INT NOT NULL,
    title NVARCHAR(255) NOT NULL,
    duration NVARCHAR(10) NULL, -- формат "mm:ss"
    track_number INT NOT NULL,
    side CHAR(1) NOT NULL, -- A или B
    CONSTRAINT FK_tracks_vinyls FOREIGN KEY (vinyl_id) REFERENCES vinyls(id) ON DELETE CASCADE
);
GO

-- Таблица статусов заказов
CREATE TABLE order_statuses (
    id INT IDENTITY(1,1) PRIMARY KEY,
    name NVARCHAR(50) NOT NULL UNIQUE,
    CONSTRAINT UQ_order_statuses_name UNIQUE (name)
);
GO

-- Таблица заказов
CREATE TABLE orders (
    id INT IDENTITY(1,1) PRIMARY KEY,
    user_id INT NOT NULL,
    order_date DATETIME2 DEFAULT GETDATE(),
    total_amount DECIMAL(10, 2) NOT NULL,
    status_id INT NOT NULL DEFAULT 1,
    shipping_address NVARCHAR(MAX) NULL,
    CONSTRAINT FK_orders_users FOREIGN KEY (user_id) REFERENCES users(id) ON DELETE CASCADE,
    CONSTRAINT FK_orders_statuses FOREIGN KEY (status_id) REFERENCES order_statuses(id) ON DELETE CASCADE
);
GO

-- Таблица содержимого заказов и позиций (товары по заказам)
CREATE TABLE order_items (
    id INT IDENTITY(1,1) PRIMARY KEY,
    order_id INT NOT NULL,
    vinyl_id INT NOT NULL,
    quantity INT NOT NULL,
    price DECIMAL(10, 2) NOT NULL,
    CONSTRAINT FK_order_items_orders FOREIGN KEY (order_id) REFERENCES orders(id) ON DELETE CASCADE,
    CONSTRAINT FK_order_items_vinyls FOREIGN KEY (vinyl_id) REFERENCES vinyls(id) ON DELETE CASCADE
);
GO

-- Таблица отзывов на пластинки
CREATE TABLE vinyl_reviews (
    id INT IDENTITY(1,1) PRIMARY KEY,
    vinyl_id INT NOT NULL,
    user_id INT NOT NULL,
    rating INT NOT NULL,
    review_text NVARCHAR(MAX) NULL,
    created_at DATETIME2 DEFAULT GETDATE(),
    CONSTRAINT FK_vinyl_reviews_vinyls FOREIGN KEY (vinyl_id) REFERENCES vinyls(id) ON DELETE CASCADE,
    CONSTRAINT FK_vinyl_reviews_users FOREIGN KEY (user_id) REFERENCES users(id) ON DELETE CASCADE,
    CONSTRAINT CK_vinyl_reviews_rating CHECK (rating BETWEEN 1 AND 5)
);
GO

-- Вставка ролей пользователей
INSERT INTO roles (name) VALUES 
('admin'),
('user'),
('manager');
GO

-- Вставка исполнителей
INSERT INTO artists (name) VALUES 
('КУОК'),
('ATL'),
('КИНО'),
('The Birthday Massacre'),
('Machine Gun Kelly'),
('Anri'),
('Ария'),
('Сплин'),
('DVRST'),
('Horus');
GO

-- Вставка музыкальных жанров
INSERT INTO genres (name) VALUES 
('Фонк'),
('Рэп'),
('Рок'),
('Метал'),
('Сити-поп'),
('Панк'),
('Джангл');
GO

-- Вставка лейблов звукозаписи
INSERT INTO labels (name) VALUES 
('G59'),
('MorozRecords'),
('Mitropoliten'),
('Мелодия');
GO

-- Вставка виниловых пластинок
INSERT INTO vinyls (title, artist_id, genre_id, label_id, release_year, cover_image_path, price, rpm, condition, quantity_in_stock) VALUES 
('ДЖАНГЛ', 1, 7, 2, 2022, '~/media/kuok_djungle.jpg', 3422.22, 33, 'Black', 1),
('Imago', 1, 2, 2, 2020, '~/media/kuok_imago.jpg', 5332.21, 33, 'Black', 2),
('Красность', 1, 2, 2, 2019, '~/media/kuok_krasnost.jpg', 2423.91, 33, 'Black', 1),
('Лимб', 2, 2, 1, 2017, '~/media/atl_limb.jpg', 12022.79, 33, 'Black', 4),
('Черный альбом', 3, 3, 4, 1987, '~/media/kino_black.jpg', 2632.12, 33, 'Black', 2),
('Pins And Needles', 4, 4, 3, 2012, '~/media/tbm_pins.jpg', 42788.00, 33, 'Black', 7),
('Walking With Strangers', 4, 4, 3, 2007, '~/media/tbm_walking.jpg', 68922.02, 33, 'Black', 3),
('Mainstream Sellout', 5, 6, 1, 2022, '~/media/mgk_sellout.jpg', 3222.54, 33, 'Clear', 9),
('Timely!!', 6, 5, 3, 2009, '~/media/anri_time.jpg', 5222.94, 33, 'Clear Blue', 6),
('Химера', 7, 3, 4, 1993, '~/media/aria_himera.jpg', 2342.04, 33, 'Orange', 1),
('Гранатовый альбом', 8, 3, 2, 2006, '~/media/splin_granat.jpg', 3922.27, 33, 'Dark Red & Black', 1),
('Echoes of Child', 9, 1, 1, 2024, '~/media/dvrst_child.jpg', 7777.77, 33, 'Black', 1),
('Рифмономикон', 10, 2, 2, 2018, '~/media/horus_rifma.jpg', 4900.00, 33, 'Black', 1);
GO

-- Добавление пользователей
INSERT INTO users (login, password, role_id, first_name, last_name, email, phone_number) VALUES 
('admin', 'admin', 1, 'Александр', 'Себежко', 'a.a.sebezhko@mpt.ru', '+79161111111'),
('urey', 'urey2', 3, 'Илья', 'Чернышев', 'i.v.chernushev@mpt.ru', '+79162222222'),
('nastya1820', '123456', 2, 'Анастасия', 'Никонова', 'a.n.nikonova@mpt.ru', '+79163333333');
GO

-- Вставка статусов заказов
INSERT INTO order_statuses (name) VALUES 
('Создан'),
('Оплачен'),
('В обработке'),
('Упакован'),
('В пути'),
('Доставлен'),
('Отменен'),
('Возврат');
GO

-- Сначала создадим заказы для существующих пользователей
INSERT INTO orders (user_id, total_amount, status_id, shipping_address) VALUES 
(1, 3422.22, 1, 'Красногорск, ул. Чайковского д.10'),
(2, 7777.77, 2, 'Москва, ул. Нежинская д.7'),
(3, 4900.00, 3, 'Москва, ул. Нахимовская д.10');
GO

-- Затем добавим товары к существующим заказам
INSERT INTO order_items (order_id, vinyl_id, quantity, price) VALUES 
(1, 1, 1, 3422.22),  -- Заказ #1 (пользователь admin), винил "ДЖАНГЛ"
(2, 12, 1, 7777.77), -- Заказ #2 (пользователь urey), винил "Echoes of Child"
(3, 13, 1, 4900.00); -- Заказ #3 (пользователь nastya1820), винил "Рифмономикон"
GO

-- Добавим отзывы от существующих пользователей
INSERT INTO vinyl_reviews (vinyl_id, user_id, rating, review_text) VALUES
(1, 3, 5, 'Отличный альбом, рекомендую всем любителям джангла!'),    -- Отзыв от nastya1820 на "ДЖАНГЛ"
(12, 2, 5, 'Просто космос, DVRST как всегда на высоте'),             -- Отзыв от urey на "Echoes of Child"
(13, 1, 4, 'Крутой альбом, хотя некоторые треки можно было бы сделать лучше'); -- Отзыв от admin на "Рифмономикон"
GO