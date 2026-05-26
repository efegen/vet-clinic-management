// Vet Klinik — istemci tarafı yardımcıları

// TR mobil telefon: okunur maske ("0 (532) 111 22 33") gösterirken,
// sunucuya ham digit ("05321112233") gönderir (Data Annotations regex'i bozmamak için).
(function () {
    function digitsOnly(value) {
        var d = (value || "").replace(/\D/g, "");
        if (d.indexOf("90") === 0 && d.length > 10) d = d.slice(2);
        if (d.indexOf("0") === 0) d = d.slice(1);
        return d.slice(0, 10); // başında 5 olan 10 haneli abone no
    }

    function formatDisplay(value) {
        var d = digitsOnly(value);
        if (d.length === 0) return "";
        var out = "0 (" + d.slice(0, 3);
        if (d.length >= 3) out += ")";
        if (d.length > 3) out += " " + d.slice(3, 6);
        if (d.length > 6) out += " " + d.slice(6, 10);
        return out;
    }

    function rawValue(value) {
        var d = digitsOnly(value);
        return d.length ? "0" + d : "";
    }

    document.addEventListener("DOMContentLoaded", function () {
        document.querySelectorAll(".phone-field").forEach(function (field) {
            var display = field.querySelector("[data-phone-display]");
            var hidden = field.querySelector("[data-phone-value]");
            if (!display || !hidden) return;

            if (hidden.value) display.value = formatDisplay(hidden.value);

            display.addEventListener("input", function () {
                hidden.value = rawValue(display.value);
                display.value = formatDisplay(display.value);
            });
        });
    });
})();
