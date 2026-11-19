// Interacciones ligeras que en el prototipo React eran estado local.
(function () {
  // Toast: se oculta solo y con el boton de cerrar.
  document.querySelectorAll('[data-toast]').forEach(function (toast) {
    var hide = function () { toast.remove(); };
    setTimeout(hide, 4000);
    var btn = toast.querySelector('[data-toast-close]');
    if (btn) btn.addEventListener('click', hide);
  });

  // Modal: clic en el fondo o tecla Escape cierra navegando a data-close-url.
  document.querySelectorAll('[data-modal]').forEach(function (modal) {
    var close = function () { window.location.href = modal.getAttribute('data-close-url'); };
    modal.addEventListener('click', function (e) { if (e.target === modal) close(); });
    document.addEventListener('keydown', function (e) { if (e.key === 'Escape') close(); });
  });

  // Selects / checkboxes que envian su formulario al cambiar.
  document.querySelectorAll('[data-autosubmit]').forEach(function (el) {
    el.addEventListener('change', function () { el.form.submit(); });
  });

  // Contador +/- (huespedes, personas). Opcionalmente recalcula un total.
  var money = function (n) { return '$' + n.toLocaleString('es-CO'); };
  document.querySelectorAll('[data-counter]').forEach(function (box) {
    var input = box.querySelector('input');
    var label = box.querySelector('[data-counter-value]');
    var min = parseInt(box.getAttribute('data-min') || '1', 10);
    var max = parseInt(box.getAttribute('data-max') || '99', 10);
    var price = parseFloat(box.getAttribute('data-price') || '0');
    var total = document.querySelector(box.getAttribute('data-total') || null);
    var warn = document.querySelector(box.getAttribute('data-warn') || null);
    var warnAbove = parseInt(box.getAttribute('data-warn-above') || '0', 10);
    var render = function () {
      var v = parseInt(input.value, 10);
      label.textContent = v;
      if (total && price) total.textContent = money(price * v);
      if (warn) warn.classList.toggle('hidden', !(warnAbove && v > warnAbove));
    };
    box.querySelectorAll('[data-step]').forEach(function (btn) {
      btn.addEventListener('click', function () {
        var v = parseInt(input.value, 10) + parseInt(btn.getAttribute('data-step'), 10);
        input.value = Math.max(min, Math.min(max, v));
        render();
      });
    });
    render();
  });

  // Casilla personalizada (confirmacion de mayoria de edad).
  document.querySelectorAll('[data-check]').forEach(function (label) {
    var input = label.querySelector('input[type=checkbox]');
    var box = label.querySelector('[data-check-box]');
    var mark = label.querySelector('[data-check-mark]');
    var on = ['bg-blue-600', 'border-blue-600'];
    var off = ['border-[#253d60]', 'group-hover:border-blue-500/50'];
    var render = function () {
      on.forEach(function (c) { box.classList.toggle(c, input.checked); });
      off.forEach(function (c) { box.classList.toggle(c, !input.checked); });
      mark.classList.toggle('hidden', !input.checked);
    };
    input.addEventListener('change', render);
    render();
  });

  // Paneles en vivo (HU-05, HU-11). El panel del servidor es un observador de las habitaciones
  // y aumenta su numero de version con cada cambio de estado. Aqui se consulta esa version
  // cada 3 segundos y, si cambio, se recarga la pagina para mostrar los datos nuevos.
  var live = document.querySelector('[data-live-url]');
  if (live && window.fetch) {
    var liveUrl = live.getAttribute('data-live-url');
    var known = live.getAttribute('data-live-version');
    setInterval(function () {
      // No se interrumpe a quien tiene un formulario abierto en un modal.
      if (document.querySelector('[data-modal]')) return;
      fetch(liveUrl, { credentials: 'same-origin', cache: 'no-store' })
        .then(function (r) { return r.ok ? r.text() : null; })
        .then(function (version) {
          if (version === null) return;
          if (known === null) { known = version; return; }
          if (version !== known) window.location.reload();
        })
        .catch(function () { /* sin conexion: se intenta de nuevo en el siguiente ciclo */ });
    }, 3000);
  }
})();
