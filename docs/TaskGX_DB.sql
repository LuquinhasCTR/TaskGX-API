CREATE DATABASE IF NOT EXISTS taskgx
CHARACTER SET utf8mb4
COLLATE utf8mb4_unicode_ci;

USE taskgx;

-- ========================================
-- Tabela: Usuarios
-- ========================================
CREATE TABLE IF NOT EXISTS Usuarios (
    ID INT AUTO_INCREMENT PRIMARY KEY,
    Nome VARCHAR(100) NOT NULL,
    Email VARCHAR(150) NOT NULL UNIQUE,
    EmailPendente VARCHAR(150) NULL,
    Senha TEXT NOT NULL,
    Avatar VARCHAR(255) NULL,
    Ativo BIT NOT NULL DEFAULT 1,
    EmailVerificado BIT NOT NULL DEFAULT 0,
    CodigoVerificacao VARCHAR(100) NULL,
    CodigoVerificacaoExpiracao DATETIME NULL,
    Criado_em DATETIME NOT NULL DEFAULT CURRENT_TIMESTAMP,
    DataAtualizacao DATETIME NOT NULL DEFAULT CURRENT_TIMESTAMP ON UPDATE CURRENT_TIMESTAMP
);

-- ========================================
-- Tabela: Listas
-- ========================================
CREATE TABLE IF NOT EXISTS Listas (
    ID INT AUTO_INCREMENT PRIMARY KEY,
    Usuario_id INT NOT NULL,
    Nome VARCHAR(100) NOT NULL,
    Cor VARCHAR(20) NULL,
    Favorita BIT NOT NULL DEFAULT 0,
    Ordem INT NOT NULL DEFAULT 0,
    DataCriacao DATETIME NOT NULL DEFAULT CURRENT_TIMESTAMP,

    CONSTRAINT FK_Listas_Usuarios
        FOREIGN KEY (Usuario_id) REFERENCES Usuarios(ID)
        ON DELETE CASCADE
        ON UPDATE CASCADE
);

-- ========================================
-- Tabela: Prioridades
-- ========================================
CREATE TABLE IF NOT EXISTS Prioridades (
    ID INT AUTO_INCREMENT PRIMARY KEY,
    Nome VARCHAR(50) NOT NULL
);

-- ========================================
-- Tabela: Tarefas
-- ========================================
CREATE TABLE IF NOT EXISTS Tarefas (
    ID INT AUTO_INCREMENT PRIMARY KEY,
    Lista_id INT NOT NULL,
    Titulo VARCHAR(200) NOT NULL,
    Descricao TEXT NULL,
    Tags VARCHAR(255) NULL,
    Prioridade_id INT NULL,
    Concluida BIT NOT NULL DEFAULT 0,
    Arquivada BIT NOT NULL DEFAULT 0,
    DataVencimento DATE NULL,
    DataCriacao DATETIME NOT NULL DEFAULT CURRENT_TIMESTAMP,
    DataAtualizacao DATETIME NOT NULL DEFAULT CURRENT_TIMESTAMP ON UPDATE CURRENT_TIMESTAMP,
    Ordem INT NOT NULL DEFAULT 0,

    CONSTRAINT FK_Tarefas_Listas
        FOREIGN KEY (Lista_id) REFERENCES Listas(ID)
        ON DELETE CASCADE
        ON UPDATE CASCADE,

    CONSTRAINT FK_Tarefas_Prioridades
        FOREIGN KEY (Prioridade_id) REFERENCES Prioridades(ID)
        ON DELETE SET NULL
        ON UPDATE CASCADE
);

-- ========================================
-- Dados iniciais de Prioridades
-- ========================================
INSERT INTO Prioridades (Nome)
SELECT 'Baixa'
WHERE NOT EXISTS (SELECT 1 FROM Prioridades WHERE Nome = 'Baixa');

INSERT INTO Prioridades (Nome)
SELECT 'Média'
WHERE NOT EXISTS (SELECT 1 FROM Prioridades WHERE Nome = 'Média');

INSERT INTO Prioridades (Nome)
SELECT 'Alta'
WHERE NOT EXISTS (SELECT 1 FROM Prioridades WHERE Nome = 'Alta');