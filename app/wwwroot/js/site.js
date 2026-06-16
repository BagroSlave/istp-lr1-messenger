// Спливні меню: кнопка з атрибутом data-popup="id" відкриває/закриває панель #id.
document.addEventListener('click', function (e) {
    var trigger = e.target.closest('[data-popup]');
    if (trigger) {
        e.preventDefault();
        e.stopPropagation();
        var pop = document.getElementById(trigger.getAttribute('data-popup'));
        document.querySelectorAll('.popup.open').forEach(function (p) { if (p !== pop) p.classList.remove('open'); });
        if (pop) pop.classList.toggle('open');
        return;
    }
    // клік поза попапом — закрити всі (крім кліків усередині самого попапа)
    if (!e.target.closest('.popup')) {
        document.querySelectorAll('.popup.open').forEach(function (p) { p.classList.remove('open'); });
    }
});

// Автопрокрутка вікна чату донизу до останнього повідомлення.
window.addEventListener('load', function () {
    var box = document.getElementById('chatScroll');
    if (box) { box.scrollTop = box.scrollHeight; }
});
