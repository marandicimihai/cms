window.sidebar = {
  // width should be a CSS length, e.g. '16rem' or '3.5rem'
  setWidth: function (width) {
    try {
      document.documentElement.style.setProperty('--sidebar-width', width);
    } catch (e) {
      console && console.error && console.error('sidebar.setWidth error', e);
    }
  }
};
