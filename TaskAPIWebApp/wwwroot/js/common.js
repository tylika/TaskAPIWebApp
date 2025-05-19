// --- wwwroot/js/common.js ---

/**
 * Відображає повідомлення для користувача.
 * @param {string} containerId - ID HTML-елемента, де буде відображено повідомлення.
 * @param {string} text - Текст повідомлення.
 * @param {string} type - Тип повідомлення ('success', 'error', 'info').
 */
function showMessage(containerId, text, type = 'success') {
    const messageContainer = document.getElementById(containerId);
    if (!messageContainer) {
        console.error(`Message container with id '${containerId}' not found.`);
        return;
    }
    messageContainer.innerHTML = ''; // Очистити попередні повідомлення
    const messageDiv = document.createElement('div');
    messageDiv.className = `message ${type}`; // Класи для стилізації (мають бути в site.css)
    messageDiv.textContent = text;
    messageContainer.appendChild(messageDiv);

    // Автоматичне приховування повідомлення через деякий час
    setTimeout(() => {
        if (messageDiv.parentNode === messageContainer) { // Перевірка, чи елемент ще існує
            messageContainer.innerHTML = '';
        }
    }, 4000); // 4 секунди
}

/**
 * Екранує спеціальні HTML-символи для безпечного відображення тексту.
 * @param {string | number | null | undefined} unsafe - Текст для екранування.
 * @returns {string} Екранований текст.
 */
function escapeHtml(unsafe) {
    if (unsafe === null || typeof unsafe === 'undefined') {
        return '';
    }
    return unsafe.toString()
        .replace(/&/g, "&amp;")
        .replace(/</g, "&lt;")
        .replace(/>/g, "&gt;")
        .replace(/"/g, "&quot;")
        .replace(/'/g, "&#039;");
}

/**
 * Обробляє помилкову відповідь від API та повертає текст помилки.
 * @param {Response} response - Об'єкт Response від fetch.
 * @returns {Promise<string>} Текст помилки.
 */
async function handleApiResponseError(response) {
    let errorMessage = `Помилка ${response.status}: ${response.statusText || 'Невідома помилка сервера'}`;
    try {
        const errorData = await response.json();
        if (errorData) {
            if (errorData.message) {
                errorMessage = errorData.message;
            } else if (errorData.title && errorData.errors) { // Стандартні помилки валідації ASP.NET Core ModelState
                const errorDetails = Object.values(errorData.errors).flat().join('; ');
                errorMessage = `${errorData.title}${errorDetails ? ': ' + errorDetails : ''}`;
            } else if (typeof errorData === 'string' && errorData.length > 0 && errorData.length < 500) { // Якщо тіло помилки просто рядок
                errorMessage = errorData;
            } else if (response.status === 400 && errorData.errors) { // Інший формат помилок валідації
                const errorDetails = Object.keys(errorData.errors)
                    .map(key => `${key}: ${errorData.errors[key].join(', ')}`)
                    .join('; ');
                errorMessage = `Помилка валідації: ${errorDetails}`;
            }
        }
    } catch (e) {
        // Якщо тіло відповіді не JSON або порожнє, залишити початкове повідомлення
        const textResponse = await response.text(); // Спробуємо прочитати як текст
        if (textResponse && textResponse.length > 0 && textResponse.length < 500) {
            errorMessage = textResponse;
        }
        console.warn('Не вдалося розпарсити тіло помилки як JSON:', e);
    }
    return errorMessage;
}

/**
 * Заповнює HTML-елемент <select> опціями з наданого масиву.
 * @param {HTMLSelectElement} selectElement - Елемент <select> для заповнення.
 * @param {Array<Object>} items - Масив об'єктів для створення опцій.
 * @param {string} valueField - Назва поля в об'єкті items, яке буде використовуватися як value для опції.
 * @param {string} textField - Назва поля в об'єкті items, яке буде використовуватися як текст опції.
 * @param {string} defaultOptionText - Текст для першої (дефолтної/порожньої) опції.
 * @param {boolean} addEmptyOptionAsDefault - Чи додавати першу порожню опцію.
 */
function populateGenericSelect(selectElement, items, valueField, textField, defaultOptionText, addEmptyOptionAsDefault = true) {
    if (!selectElement) {
        // console.warn(`populateGenericSelect: selectElement is null or undefined.`);
        return;
    }
    selectElement.innerHTML = ''; // Очищення попередніх опцій
    if (addEmptyOptionAsDefault) {
        const defaultOpt = document.createElement('option');
        defaultOpt.value = ""; // Порожнє значення для "Всі" або "Виберіть"
        defaultOpt.textContent = defaultOptionText;
        selectElement.appendChild(defaultOpt);
    }
    if (items && Array.isArray(items)) {
        items.forEach(item => {
            const option = document.createElement('option');
            option.value = item[valueField];
            option.textContent = item[textField];
            selectElement.appendChild(option);
        });
    } else {
        console.warn(`populateGenericSelect: items is not an array or is undefined for selectElement id: ${selectElement.id}`);
    }
}

/**
 * Асинхронно завантажує дані з API та заповнює елемент <select>.
 * @param {HTMLSelectElement} selectElement - Елемент <select> для заповнення.
 * @param {string} apiUrl - URL для отримання даних.
 * @param {string} valueField - Назва поля для value опції.
 * @param {string} textField - Назва поля для тексту опції.
 * @param {string} defaultOptionText - Текст для дефолтної опції.
 * @param {boolean} isFilter - Чи використовується цей селект для фільтра (впливає на текст "Завантаження...").
 */
async function populateSelectWithOptions(selectElement, apiUrl, valueField, textField, defaultOptionText, isFilter = false) {
    if (!selectElement) {
        // console.warn(`populateSelectWithOptions: selectElement for API ${apiUrl} not found on this page.`);
        return; // Просто виходимо, якщо селекта немає на сторінці
    }
    const initialOptionText = isFilter ? defaultOptionText : "Завантаження...";
    selectElement.innerHTML = `<option value="">${initialOptionText}</option>`; // Початкове повідомлення
    try {
        const response = await fetch(apiUrl);
        if (!response.ok) {
            throw new Error(`Не вдалося завантажити дані (${response.status}) для ${selectElement.id || 'селекта'} з ${apiUrl}`);
        }
        const items = await response.json();
        populateGenericSelect(selectElement, items, valueField, textField, defaultOptionText, true);
    } catch (error) {
        console.error(error.message);
        selectElement.innerHTML = `<option value="">Помилка завантаження</option>`;
        // Можна викликати showMessage тут, якщо є глобальний контейнер повідомлень
        // showMessage('messageContainer', error.message, 'error');
    }
}


// --- Логіка для мобільної навігації та активного посилання в хедері ---
document.addEventListener('DOMContentLoaded', () => {
    const mobileNavToggle = document.querySelector('.mobile-nav-toggle');
    const mainNav = document.querySelector('.main-nav'); // Селектор для <nav class="main-nav">

    if (mobileNavToggle && mainNav) {
        mobileNavToggle.addEventListener('click', () => {
            const isExpanded = mobileNavToggle.getAttribute('aria-expanded') === 'true' || false;
            mobileNavToggle.setAttribute('aria-expanded', !isExpanded);
            mainNav.classList.toggle('active'); // Додаємо/видаляємо клас для показу/приховування меню

            // Анімація "бургера" в "хрестик" (якщо стилі для цього є в site.css)
            const iconBars = mobileNavToggle.querySelectorAll('.icon-bar');
            if (mainNav.classList.contains('active')) {
                if (iconBars.length === 3) {
                    // Стилі для "хрестика" краще задавати через CSS класи,
                    // але для прикладу можна й так, якщо стилі бургера це дозволяють.
                    // Наприклад, можна додати клас .is-active до mobileNavToggle
                    // і стилізувати .icon-bar всередині .is-active.
                    // Поки що залишаю без прямої зміни стилів іконок тут,
                    // бо ти просив спростити CSS раніше.
                }
            } else {
                // Повернення до вигляду "бургера"
            }
        });
    }

    // Підсвічування активного посилання в навігації
    try {
        const currentPath = window.location.pathname;
        const navLinks = document.querySelectorAll('.main-nav a'); // Переконайся, що це правильний селектор для посилань

        navLinks.forEach(link => {
            link.classList.remove('active'); // Спочатку знімаємо клас з усіх
            const linkHref = link.getAttribute('href');

            if (linkHref) {
                // Точна відповідність або відповідність для головної сторінки
                if (linkHref === currentPath || (linkHref === '/index.html' && currentPath === '/')) {
                    link.classList.add('active');
                }
                // Для інших сторінок, якщо шлях починається з href посилання (і воно не є просто '/')
                // Це дозволяє підсвічувати /tasks/index.html, коли поточний шлях /tasks/ або /tasks/index.html
                else if (linkHref !== '/' && linkHref !== '/index.html' && currentPath.startsWith(linkHref.replace(/index\.html$/, ''))) {
                    if (currentPath.startsWith(linkHref) || currentPath.startsWith(linkHref.substring(0, linkHref.lastIndexOf('/') + 1 || linkHref.length))) {
                        link.classList.add('active');
                    }
                }
            }
        });

        // Якщо після всіх перевірок жодне посилання не активне, і ми на головній сторінці
        const activeLink = document.querySelector('.main-nav a.active');
        if (!activeLink && (currentPath === '/' || currentPath === '/index.html')) {
            const homeLink = document.querySelector('.main-nav a[href="/index.html"], .main-nav a[href="/"]');
            if (homeLink) {
                homeLink.classList.add('active');
            }
        }

    } catch (e) {
        console.error("Помилка підсвічування активного посилання:", e);
    }
});